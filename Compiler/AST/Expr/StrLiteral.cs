using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class StrLiteralAST : ExprAST {
    public string value;
    public int[] asciiValues;

    public StrLiteralAST(
        string value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.value = value;
        this.asciiValues = new int[value.Length];

        for (int i = 0; i < value.Length; i++) {
            this.asciiValues[i] = value[i];
        }
        this.type = new ArrayType<IntType>(new IntType());
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}