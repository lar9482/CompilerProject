using CompilerProj.Visitors;

/** RETURN statement */
public sealed class IRReturn : IRStmt {
    public List<IRExpr> returns;

    public IRReturn(List<IRExpr> returns) {
        this.returns = returns;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}