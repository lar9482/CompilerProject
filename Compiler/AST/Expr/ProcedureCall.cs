using CompilerProj.Visitors;

public sealed class ProcedureCallAST : ExprAST {
    public string procedureName;
    public List<ExprAST> args;

    public ProcedureCallAST(
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