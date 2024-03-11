using CompilerProj.AST;

public class ConditionalAST : StmtAST {
    public ExprAST ifCondition;
    public BlockAST ifBlock;

    public Dictionary<ExprAST, BlockAST>? elseIfConditionalBlocks;
    public BlockAST? elseBlock;

    public ConditionalAST(
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
}