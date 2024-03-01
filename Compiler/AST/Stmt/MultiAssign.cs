public class MultiAssignAST : StmtAST{
    public List<string> variableName;
    public List<ExprAST> value;

    public MultiAssignAST (
        List<string> variableName,
        List<ExprAST> value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.variableName = variableName;
        this.value = value;
    }
}