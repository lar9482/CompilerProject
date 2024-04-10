using CompilerProj.IR;
using CompilerProj.IRInterpreter.ExprStack;

public sealed class IRSimulator {
    private IRConfiguration configuration;
    private IRCompUnit compUnit;

    /** map from address to instruction */
    private Dictionary<int, IRNode> addressToInsn;

    /** map from labeled name to address */
    private Dictionary<string, int> nameToAddress;

    /** heap*/
    private int[] mem;
    
    /** heap maximum size */
    private int heapSizeMax;
    private const int DEFAULT_HEAP_SIZE = 327680;

    private ExprStack exprStack;

    private HashSet<string> libraryFunctions;

    public IRSimulator(IRCompUnit compUnit, int? heapSizeMax = null) {
        
        if (heapSizeMax == null) {
            this.heapSizeMax = DEFAULT_HEAP_SIZE;
            this.mem = new int[DEFAULT_HEAP_SIZE];
        } else {
            this.heapSizeMax = (int) heapSizeMax;
            this.mem = new int[(int) heapSizeMax];
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
    }
}