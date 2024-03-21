using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class IntLiteralAST : ExprAST {
    public int value;

    public IntLiteralAST(
        int value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.value = value;
        this.type = new IntType();
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}