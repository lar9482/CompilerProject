using CompilerProj.AST;
using CompilerProj.Visitors;

internal sealed class ConditionalAST : StmtAST {
    internal ExprAST ifCondition;
    internal BlockAST ifBlock;

    internal Dictionary<ExprAST, BlockAST>? elseIfConditionalBlocks;
    internal BlockAST? elseBlock;

    internal ConditionalAST(
        ExprAST ifCondition,
        BlockAST ifBlock,
        Dictionary<ExprAST, BlockAST>? elseIfConditionalBlocks,
        BlockAST? elseBlock,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.ifCondition = ifCondition;
        this.ifBlock = ifBlock;
        this.elseIfConditionalBlocks = elseIfConditionalBlocks;
        this.elseBlock = elseBlock;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}