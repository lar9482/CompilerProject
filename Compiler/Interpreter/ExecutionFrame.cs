using CompilerProj.IR;
using CompilerProj.IRInterpreter;

/** Holds the instruction pointer and temporary registers within an execution frame. */
public sealed class ExecutionFrame {
    private Random r;

    /** instruction pointer */
    public int IP;

    /** return values from this frame. Only used if IRReturn in this function has children*/
    public List<int> rets;

    /** local registers (register name -> value)*/
    private Dictionary<string, int> regs;

    public ExecutionFrame(int IP) {
        this.r = new Random();
        this.IP = IP;
        this.rets = new List<int>();
        this.regs = new Dictionary<string, int>();
    }

    /**
     * Fetch the value at the given register
     *
     * @param tempName name of the register
     * @return the value at the given register
     */
    public int getValueFromReg(string tempName) {
        if (!regs.ContainsKey(tempName)) {
            /* Referencing a temp before having written to it - initialize
            with garbage */
            put(tempName, r.Next(int.MinValue, int.MaxValue));
        }
        return regs[tempName];
    }

    /**
     * Store a value into the given register
     *
     * @param tempName name of the register
     * @param value value to be stored
     */
    public void put(string tempName, int value) {
        if (regs.ContainsKey(tempName)) {
            regs[tempName] = value;
        } else {
            regs.Add(tempName, value);
        }
    }
    
    /**
     * Advance the instruction pointer. Since we're dealing with a tree, this is postorder
     * traversal, one step at a time, modulo jumps.
     */
    public bool advance(IRSimulator simulator) {
        int backupIP = IP;
        simulator.executeCurrInsn(this);

        if (IP == -1) return false; /* RETURN */
        if (IP != backupIP) /* A jump was performed */ 
            return true;
        IP++;
        return true;
    }

    public void setIP(int IP) {
        this.IP = IP;
    }

    public IRNode getCurrentInsn(Dictionary<int, IRNode> addressToInsn) {
        IRNode inst = addressToInsn[IP];
        if (inst == null) throw new Exception("No next instruction.  Forgot RETURN?");
        return inst;
    }
}