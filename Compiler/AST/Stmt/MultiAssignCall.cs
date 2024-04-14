using CompilerProj.Visitors;

public sealed class MultiAssignCallAST : StmtAST {
    public readonly List<VarAccessAST> variableNames;
    public readonly string functionName;
    public readonly List<ExprAST> args;

    public MultiAssignCallAST(
        List<VarAccessAST> variableNames,
        string functionName,
        List<ExprAST> args,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.variableNames = variableNames;
        this.functionName = functionName;
        this.args = args;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}