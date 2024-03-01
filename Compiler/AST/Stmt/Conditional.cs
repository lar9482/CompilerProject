using CompilerProj.AST;

public class ConditionalAST : StmtAST {
    public ExprAST ifCondition;
    public List<ExprAST> elseIfConditions;
    public ExprAST elseCondition; 

    public BlockAST ifBlock;
    public List<BlockAST> elseIfBlocks;
    public BlockAST elseBlock;

    public ConditionalAST(
        ExprAST ifCondition,
        List<ExprAST> elseIfConditions,
        ExprAST elseCondition,
        BlockAST ifBlock,
        List<BlockAST> elseIfBlocks,
        BlockAST elseBlock,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.ifCondition = ifCondition;
        this.elseIfConditions = elseIfConditions;
        this.elseCondition = elseCondition; 
        this.ifBlock = ifBlock;
        this.elseIfBlocks = elseIfBlocks;
        this.elseBlock = elseBlock;
    }
}