
/** 
    A lowered intermediate representation for Call_M(target, args)
    where M is the number of returns.

    This node has the special side effect of updating the registers RV1,...,RVm
**/
public sealed class LIRCallM : LIRStmt {
    public LIRExpr target;
    public List<LIRExpr> args;
    public int nReturns;

    public LIRCallM(LIRExpr target, List<LIRExpr> args, int nReturns) {
        this.target = target;
        this.args = args;
        this.nReturns = nReturns;
    }

    public override void accept(LIRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(LIRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}