using CompilerProj.Visitors;

internal sealed class ReturnAST : StmtAST {
    internal List<ExprAST>? returnValues;

    internal ReturnAST(
        List<ExprAST>? returnValues,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.returnValues = returnValues;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}