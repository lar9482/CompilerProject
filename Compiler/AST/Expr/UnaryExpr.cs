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
    
    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}