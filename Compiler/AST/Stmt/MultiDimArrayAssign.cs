public class MultiDimArrayAssignAST : StmtAST{
    public MultiDimArrayAccessAST arrayAccess;
    public ExprAST value;

    public MultiDimArrayAssignAST(
        MultiDimArrayAccessAST arrayAccess,
        ExprAST value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.arrayAccess = arrayAccess;
        this.value = value;
    }
}