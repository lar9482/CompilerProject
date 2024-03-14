using CompilerProj.Types;
using CompilerProj.Visitors;

internal class CharLiteralAST : ExprAST {
    internal char charValue;
    internal int asciiValue;
    internal IntType type => new IntType();

    internal CharLiteralAST(
        char charValue,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.charValue = charValue;
        this.asciiValue = (int) charValue;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}