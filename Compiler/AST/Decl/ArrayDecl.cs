using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class ArrayDeclAST : DeclAST {
    public string name;
    public ExprAST size;
    public ArrayType<PrimitiveType> declType;
    public ExprAST[]? initialValues;

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

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}