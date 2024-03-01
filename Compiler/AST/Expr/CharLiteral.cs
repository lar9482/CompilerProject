using CompilerProj.Types;

public class CharLiteralAST : ExprAST {
    public char charValue;
    public int asciiValue;
    public IntType type => new IntType();

    public CharLiteralAST(
        char charValue,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.charValue = charValue;
        this.asciiValue = (int) charValue;
    }
}