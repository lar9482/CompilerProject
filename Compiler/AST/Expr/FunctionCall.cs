using CompilerProj.Visitors;

/*
 * A function that will return something.
 */
public sealed class FunctionCallAST : ExprAST {
    public string functionName;
    public List<ExprAST> args;

    public FunctionCallAST(
        string functionName,
        List<ExprAST> args,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.functionName = functionName;
        this.args = args;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}