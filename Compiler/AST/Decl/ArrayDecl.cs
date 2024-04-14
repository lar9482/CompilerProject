using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class ArrayDeclAST : DeclAST {
    public readonly string name;
    public readonly ExprAST size;
    public readonly ArrayType<PrimitiveType> declType;
    public readonly ExprAST[]? initialValues;

    public ArrayDeclAST(
        string name,
        ExprAST size,
        PrimitiveType type,
        ExprAST[]? initialValues,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.name = name;
        this.size = size;
        this.declType = new ArrayType<PrimitiveType>(type);
        this.initialValues = initialValues;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}