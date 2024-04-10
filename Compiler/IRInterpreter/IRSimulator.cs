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

    private IRConfiguration configuration;
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

        LabelAddressBuilder addressBuilder = new LabelAddressBuilder();
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
}