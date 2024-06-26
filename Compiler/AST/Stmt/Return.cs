using CompilerProj.Visitors;

/** 
    return <ExprList>?
**/
public sealed class ReturnAST : StmtAST {
    public readonly List<ExprAST>? returnValues;

    public ReturnAST(
        List<ExprAST>? returnValues,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.returnValues = returnValues;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}