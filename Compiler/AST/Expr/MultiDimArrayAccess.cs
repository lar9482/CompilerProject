using CompilerProj.Visitors;

public sealed class MultiDimArrayAccessAST : ExprAST {

    public string arrayName;
    public ExprAST firstIndex;
    public ExprAST secondIndex;

    public MultiDimArrayAccessAST(
        string arrayName,
        ExprAST firstIndex,
        ExprAST secondIndex,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.arrayName = arrayName;
        this.firstIndex = firstIndex;
        this.secondIndex = secondIndex;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}