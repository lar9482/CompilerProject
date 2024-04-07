using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class IntLiteralAST : ExprAST {
    public readonly int value;

    public IntLiteralAST(
        int value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.value = value;
        this.type = new IntType();
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}