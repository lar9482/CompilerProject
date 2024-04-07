/**
 * An intermediate representation for an expression evaluated under side effects ESEQ(stmt, expr)
 */

 public class IR_Eseq : IRStmt {
    public IRStmt stmt;
    public IRExpr expr;

    public IR_Eseq(IRStmt stmt, IRExpr expr) {
        this.stmt = stmt;
        this.expr = expr;
    }
 }