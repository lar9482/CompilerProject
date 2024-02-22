namespace Compiler.Runtime;

public class JmpRegInst : Inst {
    public string reg { get; }

    public JmpRegInst(string reg, string instName)
    : base(instName) {
        this.reg = reg;
    }
}