internal sealed class AssignAST : StmtAST{
    internal VarAccessAST variable;
    internal ExprAST value;

    internal AssignAST (
        VarAccessAST variable,
        ExprAST value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.variable = variable;
        this.value = value;
    }
}