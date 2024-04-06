using CompilerProj.Visitors;

public sealed class MultiAssignAST : StmtAST{

    public Dictionary<VarAccessAST, ExprAST> assignments;

    public MultiAssignAST (
        Dictionary<VarAccessAST, ExprAST> assignments,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.assignments = assignments;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}