using CompilerProj.Types;
using CompilerProj.Visitors;

internal class BoolLiteralAST : ExprAST {
    internal bool value;

    internal BoolLiteralAST(
        bool value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.value = value;
        this.type = new BoolType();
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}