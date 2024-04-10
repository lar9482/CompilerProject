using CompilerProj.Visitors;

/** An intermediate representation for a temporary register TEMP(name) */
public sealed class IRTemp : IRExpr {
    public string name;

    public IRTemp(string name) {
        this.name = name;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}