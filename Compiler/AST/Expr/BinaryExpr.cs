using CompilerProj.Visitors;

public sealed class BinaryExprAST : ExprAST {
    public ExprAST leftOperand;
    public ExprAST rightOperand;
    public BinaryExprType exprType;

    public BinaryExprAST(
        ExprAST leftOperand,
        ExprAST rightOperand,
        BinaryExprType exprType,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.leftOperand = leftOperand;
        this.rightOperand = rightOperand;
        this.exprType = exprType;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}