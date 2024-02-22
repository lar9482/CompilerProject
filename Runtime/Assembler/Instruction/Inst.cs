namespace Compiler.Runtime;

public abstract class Inst {
    public string instName { get; }

    public Inst(string instName) {
        this.instName = instName;
    }
}