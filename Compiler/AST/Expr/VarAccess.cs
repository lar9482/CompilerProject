internal sealed class VarAccessAST : ExprAST {

    internal string variableName;

    internal VarAccessAST(
        string variableName,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.variableName = variableName;
    }
}