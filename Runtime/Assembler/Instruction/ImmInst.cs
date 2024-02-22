namespace Compiler.Runtime;

public class ImmInst : Inst {
    public string reg { get; }
    public int integer { get; }

    public ImmInst(string reg, int integer, string instName) 
    : base(instName) {
        this.reg = reg;
        this.integer = integer;
    }
}