using System.Diagnostics;
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
    public StringWriter consoleOutputCapture;

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
    private const int DEFAULT_HEAP_SIZE = 32768000;

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
        libraryFunctions.Add("malloc");
        libraryFunctions.Add(IRConfiguration.OUT_OF_BOUNDS_FLAG); //Helper function for eventually detecting out of bounds errors.

        InsnMapBuilder addressBuilder = new InsnMapBuilder();
        addressBuilder.visit(compUnit);

        addressToInsn = addressBuilder.addressToInsn;
        nameToAddress = addressBuilder.nameToAddress;

        malloc(IRConfiguration.wordSize); // Waste first 4 bytes, to preserve an untouched space for null pointer.    

        // Redirecting console output to consoleOutputCapture
        consoleOutputCapture = new StringWriter();
        Console.SetOut(consoleOutputCapture);
    }

    private void storeAndReadExample() {
        List<int> dummyData = new List<int>() { 5, 6, 7, 8, 9, 10};
        int dataSize = dummyData.Count;

        int startAddress = malloc(IRConfiguration.wordSize * dataSize);
        for (int i = 0; i < dataSize; i++) {
            store(getMemoryAddress(startAddress, i), dummyData[i]);
        }
        
        for (int i = 0; i < dataSize; i++) {
            int test = read(getMemoryAddress(startAddress, i));
        }
    }

    private int getMemoryAddress(int startAddr, int index) {
        return startAddr + IRConfiguration.wordSize * index;
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
            switch(name) {
                case "lengthInt":
                case "lengthBool":
                    ret.Add(
                        read(args[0] - IRConfiguration.wordSize)
                    );
                    break;
                case "print":
                case "println":
                    int startAddr_print = args[0];
                    int printSize = read(startAddr_print - IRConfiguration.wordSize);
                    string print = "";
                    for (int i = 0; i < printSize; i++) {
                        print += (char) read(
                            getMemoryAddress(startAddr_print, i)
                        );
                    }

                    if (name == "print") {
                        Debug.Write(print);
                    } else {
                        Debug.WriteLine(print);
                    }

                    if (name == "print") {
                        Console.Write(print);
                    } else {
                        Console.WriteLine(print);
                    }
                    break;
                case "parseInt":
                    int startAddr_parseInt = args[0];
                    int parseIntSize = read(startAddr_parseInt - IRConfiguration.wordSize);
                    string intString = "";
                    for (int i = 0; i < parseIntSize; i++) {
                        intString += (char) read(getMemoryAddress(startAddr_parseInt, i));
                    }

                    int result = 0;
                    int success = 1;
                    try {
                        result = int.Parse(intString);
                    } catch {
                        success = 0;
                    }
                    ret.Add(result);
                    ret.Add(success);
                    break;
                case "unparseInt":
                    string argNumString = args[0].ToString();
                    int numDigits = argNumString.Length; 

                    int startAddr_unparseInt = malloc((numDigits+1) * IRConfiguration.wordSize);
                    store(startAddr_unparseInt, numDigits);
                    for (int i = 0; i < numDigits; i++) {
                        store(getMemoryAddress(startAddr_unparseInt, i+1), argNumString[i]);
                    }

                    ret.Add(startAddr_unparseInt + IRConfiguration.wordSize);
                    break;
                case "malloc":
                    ret.Add(malloc(args[0]));
                    break;
                case IRConfiguration.OUT_OF_BOUNDS_FLAG:
                    throw new Exception("Out of bounds!");
                case "assert":
                    if (args[0] != 1) {
                        throw new Exception("Assertation failure");
                    }
                    break;
                default:
                    throw new Exception(String.Format("Unrecognized library call: {0}", name));
            }
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
                String.Format("Could not find label {0}", name)
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
            case IRConst irConst: executeIRConst(irConst.value); break;
            case IRTemp irTemp: executeIRTemp(irTemp.name, frame); break;
            case IRBinOp irBinOp: executeIRBinOp(irBinOp.opType); break;
            case IRUnaryOp irUnaryOp: executeIRUnaryOp(irUnaryOp.opType); break;
            case IRMem irMem: executeIRMem(); break;
            case IRCall irCall: executeIRCall(irCall.args.Count, frame); break;
            case IRName irName: executeIRName(irName.name); break;
            case IRMove irMove: executeIRMove(frame); break;
            case IRCallStmt irCallStmt: executeIRCallStmt(irCallStmt.target, irCallStmt.args, frame); break;
            case IRExp irExp: executeIRExp(irExp); break;
            case IRJump irJump: executeIRJump(frame); break;
            case IRCJump irCJump: executeIRCJump(irCJump.trueLabel, irCJump.falseLabel, frame); break;
            case IRReturn irReturn: executeIRReturn(irReturn.returns.Count, frame); break;
            default:
                break;
        }
    }

    /*
     * Pushing the const onto the expr stack.
     */
    private void executeIRConst(int irConstValue) {
        exprStack.pushValue(irConstValue);
    }

    /*
     * Pushing the temp from the execution frame to the expr stack.
     */
    private void executeIRTemp(string tempName, ExecutionFrame frame) {
        exprStack.pushTemp(frame.getValueFromReg(tempName), tempName);
    }

    private void executeIRBinOp(BinOpType binOpType) {
        int rightVal = exprStack.popValue();
        int leftVal = exprStack.popValue();
        int result;

        switch(binOpType) {
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
            case BinOpType.ULT:
                uint leftUnsigned = (uint) leftVal;
                uint rightUnsigned = (uint) rightVal;
                result = (leftUnsigned < rightUnsigned) ? 1 : 0;
                break;
            default:
                throw new Exception("Invalid binary operation");
        }

        exprStack.pushValue(result);
    }

    private void executeIRUnaryOp(UnaryOpType unaryOpType) {
        int value = exprStack.popValue();
        int result;

        switch(unaryOpType) {
            case UnaryOpType.NOT: result = (value == 0) ? 1 : 0; break;
            case UnaryOpType.NEGATE: result = -value; break;
            default:
                throw new Exception("Invalid unary operation");
        }

        exprStack.pushValue(result);
    }

    private void executeIRMem() {
        int addr = exprStack.popValue();
        exprStack.pushAddress(read(addr), addr);
    }

    private void executeIRCall(int argCount, ExecutionFrame frame) {
        // Popping the argument values from the stack.
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

    private void executeIRName(string name) {
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

    private void executeIRCallStmt(IRExpr target, List<IRExpr> args, ExecutionFrame frame) {
        IRCall newCall = new IRCall(
            target, args
        );
        interpretInsn(frame, newCall);
        exprStack.popValue();
    }

    private void executeIRExp(IRExp irExp) {
        exprStack.pop();
    }

    private void executeIRJump(ExecutionFrame frame) {
        frame.setIP(exprStack.popValue());
    }

    private void executeIRCJump(string trueLabel, string falseLabel, ExecutionFrame frame) {
        int CJumpResult = exprStack.popValue();
        string labelToJumpTo;

        if (CJumpResult == 0) {
            labelToJumpTo = falseLabel;
        } else if(CJumpResult == 1) {
            labelToJumpTo = trueLabel;
        } else {
            throw new Exception(
                String.Format(
                    "Invalid value in CJump: Expected 0/1, but got {0}",
                    CJumpResult
                )
            );
        }

        frame.setIP(
            findLabel(labelToJumpTo)
        );
    }

    private void executeIRReturn(int nReturns, ExecutionFrame frame) {
        int[] rets = new int[nReturns];

        for (int i = nReturns-1; i>= 0; i--) {
            rets[i] = exprStack.popValue();
        }

        for (int i = 0; i < nReturns; i++) {
            frame.rets.Add(rets[i]);
        }

        frame.setIP(-1);
    }
}