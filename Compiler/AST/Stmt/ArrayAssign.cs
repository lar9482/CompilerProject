public class ArrayAssignAST : StmtAST{
    public string arrayName;
    public ExprAST accessValue;
    public ExprAST value;

    public ArrayAssignAST (
        string arrayName,
        ExprAST accessValue,
        ExprAST value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.arrayName = arrayName;
        this.accessValue = accessValue;
        this.value = value;
    }
}