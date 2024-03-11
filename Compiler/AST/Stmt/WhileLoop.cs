internal sealed class WhileLoopAST : StmtAST {
    internal ExprAST condition;
    internal BlockAST body;

    internal WhileLoopAST(
        ExprAST condition,
        BlockAST body,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.condition = condition;
        this.body = body;
    }
}