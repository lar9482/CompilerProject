using CompilerProj.Visitors;

/** An intermediate representation for a move statement MOVE(target, expr) */
public sealed class IRMove : IRStmt {

    public IRExpr target;
    public IRExpr src;

    public IRMove(IRExpr target, IRExpr src) {
        this.target = target;
        this.src = src;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}