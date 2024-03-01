public class ArrayAccessAST : ExprAST {

    public string arrayName;
    public ExprAST accessValue;

    public ArrayAccessAST(
        string arrayName,
        ExprAST accessValue,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.arrayName = arrayName;
        this.accessValue = accessValue;
    }
}