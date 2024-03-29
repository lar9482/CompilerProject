using CompilerProj.Visitors;

public sealed class UnaryExprAST : ExprAST {
    public ExprAST operand;
    public UnaryExprType exprType;

    public UnaryExprAST(
        ExprAST operand,
        UnaryExprType exprType,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.operand = operand;
        this.exprType = exprType;
    }
    
    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}