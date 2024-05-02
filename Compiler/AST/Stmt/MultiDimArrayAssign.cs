using CompilerProj.Visitors;


/** <arrayName> [ <Expr> ] [ <Expr> ]= <Expr> **/
public sealed class MultiDimArrayAssignAST : StmtAST{
    public readonly MultiDimArrayAccessAST arrayAccess;
    public readonly ExprAST value;

    public MultiDimArrayAssignAST(
        MultiDimArrayAccessAST arrayAccess,
        ExprAST value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.arrayAccess = arrayAccess;
        this.value = value;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}