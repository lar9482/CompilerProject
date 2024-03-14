using CompilerProj.Visitors;

internal sealed class ArrayAccessAST : ExprAST {

    internal string arrayName;
    internal ExprAST accessValue;

    internal ArrayAccessAST(
        string arrayName,
        ExprAST accessValue,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.arrayName = arrayName;
        this.accessValue = accessValue;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}