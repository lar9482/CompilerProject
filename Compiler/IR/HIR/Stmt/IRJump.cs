using CompilerProj.Visitors;

/** An intermediate representation for a transfer of control */
public sealed class IRJump : IRStmt {
    public IRExpr target;

    public IRJump(IRExpr target) {
        this.target = target;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}