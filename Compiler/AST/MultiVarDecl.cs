using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

internal sealed class MultiVarDeclAST : NodeAST {
    internal List<string> names;
    internal Dictionary<string, ExprAST?> initialValues;
    internal Dictionary<string, PrimitiveType> types;

    internal MultiVarDeclAST(
        List<string> names, 
        Dictionary<string, ExprAST?> initialValues,
        Dictionary<string, PrimitiveType> types,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.names = names;
        this.initialValues = initialValues;
        this.types = types;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}