using CompilerProj.Visitors;

public sealed class MultiAssignCallAST : StmtAST {
    public List<VarAccessAST> variableNames;
    public ProcedureCallAST call;

    public MultiAssignCallAST(
        List<VarAccessAST> variableNames,
        ProcedureCallAST call,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.variableNames = variableNames;
        this.call = call;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}