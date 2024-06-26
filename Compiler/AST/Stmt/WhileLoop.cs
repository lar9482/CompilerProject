using CompilerProj.Visitors;

/** while (<Expr>) <Block> **/
public sealed class WhileLoopAST : StmtAST {
    public readonly ExprAST condition;
    public readonly BlockAST body;

    public WhileLoopAST(
        ExprAST condition,
        BlockAST body,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.condition = condition;
        this.body = body;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}