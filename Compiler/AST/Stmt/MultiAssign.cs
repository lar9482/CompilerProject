public class MultiAssignAST : StmtAST{

    public Dictionary<VarAccessAST, ExprAST> assignments;

    public MultiAssignAST (
        Dictionary<VarAccessAST, ExprAST> assignments,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.assignments = assignments;
    }
}