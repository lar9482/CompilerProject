using CompilerProj.Visitors;

public sealed class ArrayAccessAST : ExprAST {

    public string arrayName;
    public ExprAST accessValue;

    public ArrayAccessAST(
        string arrayName,
        ExprAST accessValue,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.arrayName = arrayName;
        this.accessValue = accessValue;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}