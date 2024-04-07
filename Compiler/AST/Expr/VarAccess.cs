using CompilerProj.Visitors;

public sealed class VarAccessAST : ExprAST {

    public readonly string variableName;

    public VarAccessAST(
        string variableName,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.variableName = variableName;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}