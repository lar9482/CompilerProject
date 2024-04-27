using CompilerProj.Visitors;

/** An intermediate representation for naming a memory address */
public sealed class IRLabel : IRStmt {
    public string name;

    public IRLabel(string name) {
        this.name = name;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}