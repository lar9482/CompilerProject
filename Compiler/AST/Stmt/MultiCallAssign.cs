internal sealed class MultiAssignCallAST : StmtAST {
    internal List<VarAccessAST> variableNames;
    internal ProcedureCallAST call;

    internal MultiAssignCallAST(
        List<VarAccessAST> variableNames,
        ProcedureCallAST call,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.variableNames = variableNames;
        this.call = call;
    }
}