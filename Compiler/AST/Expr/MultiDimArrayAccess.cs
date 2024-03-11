internal sealed class MultiDimArrayAccessAST : ExprAST {

    internal string arrayName;
    internal ExprAST firstIndex;
    internal ExprAST secondIndex;

    internal MultiDimArrayAccessAST(
        string arrayName,
        ExprAST firstIndex,
        ExprAST secondIndex,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.arrayName = arrayName;
        this.firstIndex = firstIndex;
        this.secondIndex = secondIndex;
    }
}