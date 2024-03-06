public class VarAccessAST : ExprAST {

    public string variableName;

    public VarAccessAST(
        string variableName,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.variableName = variableName;
    }
}