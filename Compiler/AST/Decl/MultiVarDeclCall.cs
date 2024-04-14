using CompilerProj.Visitors;

public sealed class MultiVarDeclCallAST : DeclAST {
    public readonly List<string> names;
    public readonly Dictionary<string, PrimitiveType> declTypes;
    public readonly string functionName;
    public readonly List<ExprAST> args;

    public MultiVarDeclCallAST(
        List<string> names, 
        Dictionary<string, PrimitiveType> declTypes,
        string functionName,
        List<ExprAST> args,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.names = names;
        this.declTypes = declTypes;
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