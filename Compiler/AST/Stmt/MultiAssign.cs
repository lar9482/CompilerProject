internal sealed class MultiAssignAST : StmtAST{

    internal Dictionary<VarAccessAST, ExprAST> assignments;

    internal MultiAssignAST (
        Dictionary<VarAccessAST, ExprAST> assignments,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.assignments = assignments;
    }
}