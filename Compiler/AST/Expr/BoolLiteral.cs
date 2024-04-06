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

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}