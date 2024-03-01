public class MultiCallAST : StmtAST {
    public List<string> variableNames;
    public ProcedureCallAST call;

    public MultiCallAST(
        List<string> variableNames,
        ProcedureCallAST call,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.variableNames = variableNames;
        this.call = call;
    }
}