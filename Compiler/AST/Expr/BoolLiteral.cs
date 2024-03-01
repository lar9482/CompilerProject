using CompilerProj.Types;

public class BoolLiteralAST : ExprAST {
    public bool value;
    public BoolType type => new BoolType();

    public BoolLiteralAST(
        bool value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.value = value;
    }
}