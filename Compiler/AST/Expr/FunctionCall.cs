using CompilerProj.Visitors;

/*
 * A function that will return something.
 */
public sealed class FunctionCallAST : ExprAST {
    public readonly string functionName;
    public readonly List<ExprAST> args;

    public FunctionCallAST(
        string functionName,
        List<ExprAST> args,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

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