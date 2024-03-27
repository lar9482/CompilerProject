using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class MultiVarDeclAST : DeclAST {
    public List<string> names;
    public Dictionary<string, ExprAST?> initialValues;
    public Dictionary<string, PrimitiveType> declTypes;

    public MultiVarDeclAST(
        List<string> names, 
        Dictionary<string, ExprAST?> initialValues,
        Dictionary<string, PrimitiveType> declTypes,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.names = names;
        this.initialValues = initialValues;
        this.declTypes = declTypes;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}