/** 
    A lowered intermediate representation for Return(e0, e1,...,eN)
**/
public sealed class LIRReturn : LIRStmt {
    public List<LIRExpr> returns;

    public LIRReturn(List<LIRExpr> returns) {
        this.returns = returns;
    }
}