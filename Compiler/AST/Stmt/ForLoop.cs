using CompilerProj.Visitors;

public sealed class ForLoopAST : StmtAST {
    public readonly StmtAST initialize;
    public readonly ExprAST condition;
    public readonly StmtAST iterate;

    public readonly BlockAST block; 

    public ForLoopAST(
        StmtAST initialize,
        ExprAST condition,
        StmtAST iterate,
        BlockAST block,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.initialize = initialize;
        this.condition = condition;
        this.iterate = iterate;
        this.block = block;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}