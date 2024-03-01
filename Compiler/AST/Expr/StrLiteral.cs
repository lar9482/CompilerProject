using CompilerProj.Types;

public class StrLiteralAST : ExprAST {
    public string value;
    public int[] asciiValues;
    public ArrayType<IntType> type => new ArrayType<IntType>(new IntType());

    public StrLiteralAST(
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