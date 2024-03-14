using CompilerProj.Types;
using CompilerProj.Visitors;

internal class BoolLiteralAST : ExprAST {
    internal bool value;
    internal BoolType type => new BoolType();

    internal BoolLiteralAST(
        bool value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.value = value;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}