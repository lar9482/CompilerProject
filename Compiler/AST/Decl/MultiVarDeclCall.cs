using CompilerProj.Visitors;

public sealed class MultiVarDeclCallAST : DeclAST {
    public List<string> names;
    public Dictionary<string, PrimitiveType> types;
    public FunctionCallAST functionCall;

    public MultiVarDeclCallAST(
        List<string> names, 
        Dictionary<string, PrimitiveType> types,
        FunctionCallAST functionCall,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.names = names;
        this.types = types;
        this.functionCall = functionCall;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}