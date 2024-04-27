using CompilerProj.Visitors;

public sealed class IRName : IRExpr {
    public string name;

    public IRName(string name) {
        this.name = name;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}