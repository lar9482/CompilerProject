public class MultiCallAST : StmtAST {
    public List<string> variableNames;
    public string procedureName;
    public List<ExprAST> args;

    public MultiCallAST(
        List<string> variableNames,
        string procedureName,
        List<ExprAST> args,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.variableNames = variableNames;
        this.procedureName = procedureName;
        this.args = args;
    }
}