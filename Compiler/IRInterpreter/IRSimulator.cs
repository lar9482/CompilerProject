using System.Runtime.Versioning;
using CompilerProj.IR;
using CompilerProj.IRInterpreter.ExprStack;

/*
 * WARNING:
 * For simplicity sake in testing the IR, no 'free' function is implemented for 
 * releasing memory from 'malloc'. 
 
 * This shouldn't be terrible issue because
 * most programs should (hopefully) not allocate too much memory.
 *
 * Memory leaks go brrrrrrrrrrrrrr!
 */
public sealed class IRSimulator {
    private Random r = new Random();
    private IRCompUnit compUnit;

    /** map from address to instruction */
    private Dictionary<int, IRNode> addressToInsn;

    /** map from labeled name to address */
    private Dictionary<string, int> nameToAddress;

    /** heap */
    private List<byte> mem;
    
    /** heap maximum size. The maximum number of words supported */
    private int heapSizeMax;
    private const int DEFAULT_HEAP_SIZE = 327680;

    private ExprStack exprStack;

    private HashSet<string> libraryFunctions;

    public IRSimulator(IRCompUnit compUnit, int? heapSizeMax = null) {
        
        if (heapSizeMax == null) {
            this.heapSizeMax = DEFAULT_HEAP_SIZE;
            this.mem = new List<byte>();
        } else {
            this.heapSizeMax = (int) heapSizeMax;
            this.mem =  new List<byte>();
        }

        this.compUnit = compUnit;

        this.exprStack = new ExprStack();
        this.libraryFunctions = new HashSet<string>();

        // IO Functions
        libraryFunctions.Add("print");
        libraryFunctions.Add("println");
        libraryFunctions.Add("readln");
        libraryFunctions.Add("getchar");
        libraryFunctions.Add("eof");

        // Sys call functions
        libraryFunctions.Add("parseInt");
        libraryFunctions.Add("unparseInt");
        libraryFunctions.Add("lengthInt");
        libraryFunctions.Add("lengthBool");
        libraryFunctions.Add("assert");

        //Support of memory on the heap
        libraryFunctions.Add("alloc");
        libraryFunctions.Add("outOfBounds"); //Helper function for eventually detecting out of bounds errors.

        InsnMapBuilder addressBuilder = new InsnMapBuilder();
        addressBuilder.visit(compUnit);

        addressToInsn = addressBuilder.addressToInsn;
        nameToAddress = addressBuilder.nameToAddress;

        malloc(IRConfiguration.wordSize); // Waste first 4 bytes, to preserve an untouched space for null pointer.    
    }

    private void storeAndReadExample() {
        List<int> dummyData = new List<int>() { 5, 6, 7, 8, 9, 10};
        int dataSize = dummyData.Count;

        int startAddress = calloc(IRConfiguration.wordSize * dataSize);
        for (int i = 0; i < dataSize; i++) {
            store(startAddress + IRConfiguration.wordSize * i, dummyData[i]);
        }
        
        for (int i = 0; i < dataSize; i++) {
            int test = read(startAddress + IRConfiguration.wordSize * i);
        }
    }

    /**
     * Allocate a specified amount of bytes on the heap.
     * NOTE: All memory allocated is automatically word-aligned.
     *
     * @param size the number of bytes to be allocated
     * @return the starting address of the allocated region on the heap
     */
    private int malloc(int size) {
        if (size < 0) {
            throw new Exception("Invalid size");
        }
        if (size % IRConfiguration.wordSize != 0) {
            throw new Exception(
                String.Format(
                    "Can only allocate in chunks of {0} bytes", IRConfiguration.wordSize
                )
            );
        }
        int retAddress = mem.Count;

        if (mem.Count + size > heapSizeMax) 
            throw new Exception("Out of heap!");
        
        for (int i = 0; i < size; i++) {
            mem.Add((byte) r.Next(0, 255));
        }

        return retAddress;
    }

    /**
     * Allocate a specified amount of bytes on the heap and initialize them to 0.
     *
     * @param size the number of bytes to be allocated
     * @return the starting address of the allocated region on the heap
     */
    public int calloc(int size) {
        int retAddress = malloc(size);

        for (int i = retAddress; i < retAddress + size; i++) {
            mem[i] = 0;
        }
        return retAddress;
    }

    /**
     * Read a value at the specified location on the heap
     *
     * @param addr the address to be read
     * @return the value at {@code addr}
     */
    public int read(int addr) {
        if (addr < 0) throw new Exception("Attempting to read negative addresses. Invalid");
        if (addr % IRConfiguration.wordSize != 0)
            throw new Exception(
                String.Format(
                    "Unaliged memory access: {0} (word size = {1})",
                    addr, IRConfiguration.wordSize
                )
            );

        if (addr >= mem.Count) throw new Exception("Attempting to read past end of heap");

        byte[] valueBytes = new byte[4];
        valueBytes[0] = mem[addr];
        valueBytes[1] = mem[addr+1];
        valueBytes[2] = mem[addr+2];
        valueBytes[3] = mem[addr+3];

        return BitConverter.ToInt32(valueBytes, 0);
    }

    /**
     * Write a value at the specified location on the heap
     *
     * @param addr the address to be written
     * @param value the value to be written
     */
    public void store(int addr, int value) {
        if (addr < 0) throw new Exception("Attempting to read negative addresses. Invalid");
        if (addr % IRConfiguration.wordSize != 0)
            throw new Exception(
                String.Format(
                    "Unaliged memory access: {0} (word size = {1})",
                    addr, IRConfiguration.wordSize
                )
            );

        if (addr >= mem.Count) throw new Exception("Attempting to store past end of heap");

        byte[] valueBytes = BitConverter.GetBytes(value);
        mem[addr+0] = valueBytes[0];
        mem[addr+1] = valueBytes[1];
        mem[addr+2] = valueBytes[2];
        mem[addr+3] = valueBytes[3];
    }

    /**
     * Simulate a function call, throwing away all returned values past the first All arguments to
     * the function call are passed via registers with prefix {@link
     * Configuration#ABSTRACT_ARG_PREFIX} and indices starting from 1.
     *
     * TO ACCESS MULTIPLE RETURNS, YOU MUST GET THEM FROM THE EXECUTION FRAME!!!!!
     * @param name name of the function call
     * @param args arguments to the function call
     * @return the value that would be in register {@link Configuration#ABSTRACT_RET_PREFIX} index 1
     */
    public int call(string name, int[] args) {
        ExecutionFrame frame = new ExecutionFrame(-1);
        int retVal = call(frame, name, args);

        Console.WriteLine();
        return retVal;
    }

    /**
     * Simulate a function call. All arguments to the function call are passed via registers with
     * prefix {@link Configuration#ABSTRACT_ARG_PREFIX} and indices starting from 1. The function
     * call should return the results via registers with prefix {@link
     * Configuration#ABSTRACT_RET_PREFIX} and indices starting from 1.
     *
     * TO ACCESS MULTIPLE RETURNS, YOU MUST GET THEM FROM THE EXECUTION FRAME!!!!!
     * @param parent parent call frame to write _RET values to
     * @param name name of the function call
     * @param args arguments to the function call
     * @return the value of register {@link Configuration#ABSTRACT_RET_PREFIX} index 1
     */
    public int call(ExecutionFrame parentFunction, string name, int[] args) {
        if (libraryFunctions.Contains(name)) {
            List<int> ret = libraryCall(name, args);

            for (int i = 0; i < ret.Count; i++) {
                parentFunction.put(
                    IRConfiguration.ABSTRACT_RET_PREFIX + (i+1),
                    ret[i]
                );
            }

            if (ret.Count > 0) {
                return ret[0];
            } else {
                return 0;
            }
        } else {
            return IRFunctionCall(parentFunction, name, args);
        }
    }

    /**
     * Simulate a library function call, returning the list of returned values
     *
     * @param name name of the function call
     * @param args arguments to the function call, which may include the pointer to the location of
     *     multiple results
     */

    private List<int> libraryCall(string name, int[] args) {
        List<int> ret = new List<int>();

        try {
            return ret;
        } catch (IOException e) {
            throw new Exception(e.Message);
        }
    }

    private int IRFunctionCall(ExecutionFrame parentFunction, string name, int[] args) {
        
        if (!compUnit.functions.ContainsKey(name)) {
            throw new Exception(
                String.Format(
                    "{0} was never declared", name
                )
            );
        }

        //Creating a new stack frame
        int IP = findLabel(name);
        ExecutionFrame frame = new ExecutionFrame(IP);

        //Passing remaining arguments into the stack frame registers.
        for (int i = 0; i < args.Length; i++) {
            frame.put(
                IRConfiguration.ABSTRACT_ARG_PREFIX + (i+1),
                args[i]
            );
        }

        //Simulate the IR execution!!!
        while (frame.advance(this));

        //Send the child frame's returns to the parent.
        for (int i = 0; i < frame.rets.Count; i++) {
            parentFunction.put(
                IRConfiguration.ABSTRACT_RET_PREFIX + (i+1),
                frame.rets[i]
            );
        }

        if (frame.rets.Count == 0) {
            return 0;
        } else {
            return frame.rets[0];
        }
    }

    /**
     * @param name name of the label
     * @return the address at the named label
     */
    private int findLabel(string name) {
        if (!nameToAddress.ContainsKey(name)) 
            throw new Exception(
                String.Format("Could not find label {0}!", name)
            );
        
        return nameToAddress[name];
    }

    /*
     * This is where the meat and potatoes of the IR is executed.
     */
    public void executeCurrInsn(ExecutionFrame frame) {
        interpretInsn(frame, frame.getCurrentInsn(addressToInsn));
    }

    private void interpretInsn(ExecutionFrame frame, IRNode insn) {
        switch(insn) {
            case IRConst irConst: executeIRConst(irConst); break;
            case IRTemp irTemp: executeIRTemp(irTemp, frame); break;
            case IRBinOp irBinOp: executeIRBinOp(irBinOp); break;
            case IRUnaryOp irUnaryOp: executeIRUnaryOp(irUnaryOp); break;
            case IRMem irMem:
                break;
            case IRCall irCall: executeIRCall(irCall, frame); break;
            case IRName irName: executeIRName(irName); break;
            case IRMove irMove: executeIRMove(frame); break;
            case IRCallStmt irCallStmt: executeIRCallStmt(irCallStmt, frame); break;
            case IRExp irExp:
                break;
            case IRJump irJump:
                break;
            case IRCJump irCJump:
                break;
            case IRReturn irReturn: executeIRReturn(irReturn, frame); break;
            default:
                break;
        }
    }

    /*
     * Pushing the const onto the expr stack.
     */
    private void executeIRConst(IRConst irConst) {
        exprStack.pushValue(irConst.value);
    }

    /*
     * Pushing the temp from the execution frame to the expr stack.
     */
    private void executeIRTemp(IRTemp irTemp, ExecutionFrame frame) {
        string tempName = irTemp.name;
        exprStack.pushTemp(frame.getValueFromReg(tempName), tempName);
    }

    private void executeIRBinOp(IRBinOp binOp) {
        int rightVal = exprStack.popValue();
        int leftVal = exprStack.popValue();
        int result;

        switch(binOp.opType) {
            case BinOpType.ADD: result = leftVal + rightVal; break;
            case BinOpType.SUB: result = leftVal - rightVal; break;
            case BinOpType.MUL: result = leftVal * rightVal; break;
            case BinOpType.DIV: 
                if (rightVal == 0) throw new Exception("Division by zero");
                result = (int) (leftVal / rightVal);
                break;
            case BinOpType.MOD:
                if (rightVal == 0) throw new Exception("Division by zero");
                result = leftVal % rightVal;
                break;
            case BinOpType.AND: result = leftVal & rightVal; break;
            case BinOpType.OR: result = leftVal | rightVal; break;
            case BinOpType.XOR: result = leftVal ^ rightVal; break;
            case BinOpType.LSHIFT: result = leftVal << rightVal; break;
            case BinOpType.RSHIFT: result = leftVal >> rightVal; break;
            case BinOpType.EQ: result = (leftVal == rightVal) ? 1 : 0; break;
            case BinOpType.NEQ: result = (leftVal != rightVal) ? 1 : 0; break;
            case BinOpType.LT: result = (leftVal < rightVal) ? 1 : 0; break;
            case BinOpType.GT: result = (leftVal > rightVal) ? 1 : 0; break;
            case BinOpType.LEQ: result = (leftVal <= rightVal) ? 1 : 0; break;
            case BinOpType.GEQ: result = (leftVal >= rightVal) ? 1 : 0; break;
            default:
                throw new Exception("Invalid binary operation");
        }

        exprStack.pushValue(result);
    }

    private void executeIRUnaryOp(IRUnaryOp unaryOp) {
        int value = exprStack.popValue();
        int result;

        switch(unaryOp.opType) {
            case UnaryOpType.NOT: result = (value == 0) ? 1 : 0; break;
            case UnaryOpType.NEGATE: result = -value; break;
            default:
                throw new Exception("Invalid unary operation");
        }

        exprStack.pushValue(result);
    }

    private void executeIRCall(IRCall irCall, ExecutionFrame frame) {
        // Popping the argument values from the stack.
        int argCount = irCall.args.Count;
        int[] args = new int[argCount];
        for (int i = argCount-1; i>=0; i--) args[i] = exprStack.popValue();

        // Either popping the name or the address of the function to be called.
        StackItem target = exprStack.pop();
        string targetName;
        if (target.type == StackItemType.NAME) {
            if (target.name == null) {
                throw new Exception("Tried to access a null name");
            }
            targetName = target.name;
        } else if (addressToInsn.ContainsKey(target.value)) {
            IRNode node = addressToInsn[target.value];
            if (node.GetType() == typeof(IRFuncDecl)) {
                targetName = ((IRFuncDecl) node).name;
            } else { throw new Exception("Tried to call a non-function"); }
        } else {
            throw new Exception(String.Format(
                "Invalid function call: (target {0} is unknown)", target.value
            ));
        }

        int retVal = call(frame, targetName, args);
        exprStack.pushValue(retVal);
    }

    private void executeIRName(IRName irName) {
        string name = irName.name;
        exprStack.pushName(
            libraryFunctions.Contains(name) ? -1 : findLabel(name),
            name
        );
    }

    /*
     * Popping the computed src and target from the expr stack, performing move, then storing the result on the frame.
     */
    private void executeIRMove(ExecutionFrame frame) {
        int srcValue = exprStack.popValue();
        StackItem targetItem = exprStack.pop();

        switch(targetItem.type) {
            case StackItemType.MEM:
                store(targetItem.address, srcValue);
                break;
            case StackItemType.TEMP:
                if (targetItem.temp == null) {
                    throw new Exception("TEMP is Empty");
                }
                frame.put(targetItem.temp, srcValue);
                break;
            default:
                throw new Exception("Invalid MOVE!");
        }
    }

    private void executeIRCallStmt(IRCallStmt irCallStmt, ExecutionFrame frame) {
        IRCall newCall = new IRCall(
            irCallStmt.target, irCallStmt.args
        );
        interpretInsn(frame, newCall);
        exprStack.popValue();
    }

    private void executeIRReturn(IRReturn irReturn, ExecutionFrame frame) {
        int argCount = irReturn.returns.Count;
        int[] rets = new int[argCount];

        for (int i = argCount-1; i>= 0; i--) {
            rets[i] = exprStack.popValue();
        }

        for (int i = 0; i < argCount; i++) {
            frame.rets.Add(rets[i]);
        }

        frame.setIP(-1);
    }
}