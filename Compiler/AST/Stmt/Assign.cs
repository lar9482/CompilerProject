using CompilerProj.Visitors;

public sealed class AssignAST : StmtAST{
    public VarAccessAST variable;
    public ExprAST value;

    public AssignAST (
        VarAccessAST variable,
        ExprAST value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.variable = variable;
        this.value = value;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}