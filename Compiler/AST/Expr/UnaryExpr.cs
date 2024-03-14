using CompilerProj.Visitors;

internal sealed class UnaryExprAST : ExprAST {
    internal ExprAST operand;
    internal UnaryExprType exprType;

    internal UnaryExprAST(
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