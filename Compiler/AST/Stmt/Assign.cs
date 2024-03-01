public class AssignAST : StmtAST{
    public string variableName;
    public ExprAST value;

    public AssignAST (
        string variableName,
        ExprAST value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.variableName = variableName;
        this.value = value;
    }
}