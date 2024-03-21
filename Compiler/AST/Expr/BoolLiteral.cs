using CompilerProj.Types;
using CompilerProj.Visitors;

public class BoolLiteralAST : ExprAST {
    public bool value;

    public BoolLiteralAST(
        bool value,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.value = value;
        this.type = new BoolType();
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}