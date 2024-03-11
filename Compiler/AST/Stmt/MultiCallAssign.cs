internal sealed class MultiCallAST : StmtAST {
    internal List<string> variableNames;
    internal ProcedureCallAST call;

    internal MultiCallAST(
        List<string> variableNames,
        ProcedureCallAST call,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.variableNames = variableNames;
        this.call = call;
    }
}