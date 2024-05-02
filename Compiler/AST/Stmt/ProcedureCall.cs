using CompilerProj.Visitors;

/*
 * A function that will not return anything.
 *
 * <procedureName>(<param1>, ... , <paramN>)
 */
public sealed class ProcedureCallAST : StmtAST {
    public readonly string procedureName;
    public readonly List<ExprAST> args;

    public ProcedureCallAST(
        string procedureName,
        List<ExprAST> args,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.procedureName = procedureName;
        this.args = args;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}