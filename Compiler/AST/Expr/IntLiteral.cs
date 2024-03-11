using CompilerProj.Types;

internal sealed class IntLiteralAST : ExprAST {
    internal int value;
    internal IntType type => new IntType();

    internal IntLiteralAST(
        int value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.value = value;
    }
}