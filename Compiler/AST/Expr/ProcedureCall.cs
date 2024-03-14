using CompilerProj.Visitors;

internal sealed class ProcedureCallAST : ExprAST {
    internal string procedureName;
    internal List<ExprAST> args;

    internal ProcedureCallAST(
        string procedureName,
        List<ExprAST> args,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.procedureName = procedureName;
        this.args = args;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}