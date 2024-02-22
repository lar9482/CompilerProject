namespace Compiler.Runtime;

public class JmpLabelInst : Inst {
    public string label { get; }

    public JmpLabelInst(string label, string instName)
    : base(instName) {
        this.label = label;
    }
}