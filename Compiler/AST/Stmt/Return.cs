public class ReturnAST : StmtAST {
    public List<ExprAST>? returnValues;

    public ReturnAST(
        List<ExprAST>? returnValues,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.returnValues = returnValues;
    }
}