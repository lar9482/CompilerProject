using CompilerProj.Visitors;

public sealed class MultiDimArrayAssignAST : StmtAST{
    public MultiDimArrayAccessAST arrayAccess;
    public ExprAST value;

    public MultiDimArrayAssignAST(
        MultiDimArrayAccessAST arrayAccess,
        ExprAST value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.arrayAccess = arrayAccess;
        this.value = value;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}