namespace Compiler.Runtime;

public class RegInst : Inst {
    public string reg1 { get; }
    public string reg2 { get; }

    public RegInst(string reg1, string reg2, string instName) 
    : base(instName) {
        this.reg1 = reg1;
        this.reg2 = reg2;
    }
}