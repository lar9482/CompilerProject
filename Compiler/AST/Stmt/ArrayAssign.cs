using CompilerProj.Visitors;

public sealed class ArrayAssignAST : StmtAST{
    public ArrayAccessAST arrayAccess;
    public ExprAST value;

    public ArrayAssignAST (
        ArrayAccessAST arrayAccess,
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