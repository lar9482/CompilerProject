using CompilerProj.Visitors;

/** An intermediate representation for a function call CALL(e_target, e_1, ..., e_n) */
public sealed class IRCall : IRExpr {
    public IRExpr target;
    public List<IRExpr> args;

    public IRCall(IRExpr target, List<IRExpr> args) {
        this.target = target;
        this.args = args;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}