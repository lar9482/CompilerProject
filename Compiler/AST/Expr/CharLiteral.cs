using CompilerProj.Types;
using CompilerProj.Visitors;

public class CharLiteralAST : ExprAST {
    public char charValue;
    public int asciiValue;

    public CharLiteralAST(
        char charValue,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.charValue = charValue;
        this.asciiValue = (int) charValue;
        this.type = new IntType();
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}