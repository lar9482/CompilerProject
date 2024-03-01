using CompilerProj.Types;

public class IntLiteralAST : ExprAST {
    public int value;
    public IntType type => new IntType();

    public IntLiteralAST(
        int value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.value = value;
    }
}