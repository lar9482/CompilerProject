using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class ArrayAST : NodeAST {
    public string name;
    public ExprAST? size;
    public ArrayType<PrimitiveType> type;
    public ExprAST[]? initialValues;

    public ArrayAST(
        string name,
        ExprAST size,
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