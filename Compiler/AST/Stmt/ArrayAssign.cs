using CompilerProj.Visitors;

internal sealed class ArrayAssignAST : StmtAST{
    internal ArrayAccessAST arrayAccess;
    internal ExprAST value;

    internal ArrayAssignAST (
        ArrayAccessAST arrayAccess,
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