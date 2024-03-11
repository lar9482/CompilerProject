using CompilerProj.Types;

internal sealed class StrLiteralAST : ExprAST {
    internal string value;
    internal int[] asciiValues;
    internal ArrayType<IntType> type => new ArrayType<IntType>(new IntType());

    internal StrLiteralAST(
        string value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.value = value;
        this.asciiValues = new int[value.Length];

        for (int i = 0; i < value.Length; i++) {
            this.asciiValues[i] = value[i];
        }
    }
}