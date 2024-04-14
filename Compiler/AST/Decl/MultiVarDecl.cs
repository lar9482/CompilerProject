using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class MultiVarDeclAST : DeclAST {
    public readonly List<string> names;
    public readonly Dictionary<string, ExprAST?> initialValues;
    public readonly Dictionary<string, PrimitiveType> declTypes;

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

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}