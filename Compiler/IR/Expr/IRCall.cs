
/** An intermediate representation for a function call CALL(e_target, e_1, ..., e_n) */
public sealed class IRCall : IRExpr {
    public IRExpr target;
    public List<IRExpr> args;

    public IRCall(IRExpr target, List<IRExpr> args) {
        this.target = target;
        this.args = args;
    }
}