public class ArrayAssignAST : StmtAST{
    public ArrayAccessAST arrayAccess;
    public ExprAST value;

    public ArrayAssignAST (
        ArrayAccessAST arrayAccess,
        ExprAST value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.arrayAccess = arrayAccess;
        this.value = value;
    }
}