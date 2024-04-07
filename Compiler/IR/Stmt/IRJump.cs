/** An intermediate representation for a transfer of control */
public sealed class IRJump : IRStmt {
    public IRExpr target;

    public IRJump(IRExpr target) {
        this.target = target;
    }
}