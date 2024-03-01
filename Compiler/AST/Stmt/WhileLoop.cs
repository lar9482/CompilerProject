public class WhileLoopAST : StmtAST {
    public ExprAST condition;
    public BlockAST body;

    public WhileLoopAST(
        ExprAST condition,
        BlockAST body,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.condition = condition;
        this.body = body;
    }
}