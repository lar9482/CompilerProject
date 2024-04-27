using CompilerProj.Visitors;

/**
 * An intermediate representation for evaluating an expression for side effects, discarding the
 * result EXP(e)
 */

 public sealed class IRExp : IRStmt {
    public IRExpr expr;

    public IRExp(IRExpr expr) {
        this.expr = expr;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
 } 