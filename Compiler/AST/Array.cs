using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

internal sealed class ArrayAST : NodeAST {
    internal string name;
    internal int size;
    internal ArrayType<PrimitiveType> type;
    internal ExprAST[]? initialValues;

    internal ArrayAST(
        string name,
        int size,
        PrimitiveType type,
        ExprAST[]? initialValues,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.name = name;
        this.size = size;
        this.type = new ArrayType<PrimitiveType>(type);
        this.initialValues = initialValues;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}