using CompilerProj.Visitors;

internal sealed class BinaryExprAST : ExprAST {
    internal ExprAST leftOperand;
    internal ExprAST rightOperand;
    internal BinaryExprType exprType;

    internal BinaryExprAST(
        ExprAST leftOperand,
        ExprAST rightOperand,
        BinaryExprType exprType,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.leftOperand = leftOperand;
        this.rightOperand = rightOperand;
        this.exprType = exprType;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}