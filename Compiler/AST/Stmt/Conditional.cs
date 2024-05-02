using CompilerProj.AST;
using CompilerProj.Visitors;

/*
 * The following constructs:
 * if (<Expr>) <block>
 * if (<Expr>) <block> <else> <block>
 * if (<Expr>) <block> else if (<Expr>) <block> ... else if (<Expr>) <block>
 * if (<Expr>) <block> else if (<Expr>) <block> ... else if (<Expr>) <block> else <block>
 */
public sealed class ConditionalAST : StmtAST {
    public readonly ExprAST ifCondition;
    public readonly BlockAST ifBlock;

    public readonly Dictionary<ExprAST, BlockAST>? elseIfConditionalBlocks;
    public readonly BlockAST? elseBlock;

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

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}