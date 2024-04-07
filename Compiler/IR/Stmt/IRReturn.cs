
/** RETURN statement */
public sealed class IRReturn : IRStmt {
    public List<IRExpr> returns;

    public IRReturn(List<IRExpr> returns) {
        this.returns = returns;
    }
}