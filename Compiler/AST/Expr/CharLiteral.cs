using CompilerProj.Types;
using CompilerProj.Visitors;

public class CharLiteralAST : ExprAST {
    public readonly char charValue;
    public readonly int asciiValue;

    public CharLiteralAST(
        char charValue,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.charValue = charValue;
        this.asciiValue = (int) charValue;
        this.type = new IntType();
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}