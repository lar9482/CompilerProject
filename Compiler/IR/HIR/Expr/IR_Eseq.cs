using CompilerProj.Visitors;

/**
 * An intermediate representation for an expression evaluated under side effects ESEQ(stmt, expr)
 */

 public class IR_Eseq : IRExpr {
    public IRStmt stmt;
    public IRExpr expr;

    public IR_Eseq(IRStmt stmt, IRExpr expr) {
        this.stmt = stmt;
        this.expr = expr;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
 }