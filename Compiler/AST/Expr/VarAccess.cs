using CompilerProj.Visitors;

public sealed class VarAccessAST : ExprAST {

    public string variableName;

    public VarAccessAST(
        string variableName,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.variableName = variableName;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}