using CompilerProj.Visitors;

/**
 * An intermediate representation for a call statement. t_1, t_2, _, t_4 = CALL(e_target, e_1, ...,
 * e_n) where n = n_returns.
 */

 public class IRCallStmt : IRStmt {
    public IRExpr target;
    public List<IRExpr> args;
    public int nReturns;

    public IRCallStmt(IRExpr target, List<IRExpr> args, int nReturns) {
        this.target = target;
        this.args = args;
        this.nReturns = nReturns;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
 }