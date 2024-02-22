namespace Compiler.Runtime;

public class JmpBranchInst : Inst {
    public string reg1 { get; }
    public string reg2 { get; }

    public string label { get; }

    public JmpBranchInst(string reg1, string reg2, string label, string instName) 
    : base(instName) {
        this.reg1 = reg1;
        this.reg2 = reg2;
        this.label = label;
    }
}