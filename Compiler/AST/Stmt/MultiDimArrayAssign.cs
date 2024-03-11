internal sealed class MultiDimArrayAssignAST : StmtAST{
    internal MultiDimArrayAccessAST arrayAccess;
    internal ExprAST value;

    internal MultiDimArrayAssignAST(
        MultiDimArrayAccessAST arrayAccess,
        ExprAST value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.arrayAccess = arrayAccess;
        this.value = value;
    }
}