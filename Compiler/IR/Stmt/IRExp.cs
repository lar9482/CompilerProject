/**
 * An intermediate representation for evaluating an expression for side effects, discarding the
 * result EXP(e)
 */

 public sealed class IRExp : IRStmt {
    public IRExpr expr;

    public IRExp(IRExpr expr) {
        this.expr = expr;
    }
 } 