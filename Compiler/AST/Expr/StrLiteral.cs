using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class StrLiteralAST : ExprAST {
    public readonly string value;
    public readonly int[] asciiValues;

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

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}