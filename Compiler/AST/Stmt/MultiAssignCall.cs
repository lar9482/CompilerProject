using CompilerProj.Visitors;

public sealed class MultiAssignCallAST : StmtAST {
    public List<VarAccessAST> variableNames;
    public FunctionCallAST call;

    public MultiAssignCallAST(
        List<VarAccessAST> variableNames,
        FunctionCallAST call,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.variableNames = variableNames;
        this.call = call;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}