using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

internal sealed class MultiDimArrayAST : NodeAST {
    internal string name;
    internal int rowSize;
    internal int colSize;
    internal MultiDimArrayType<PrimitiveType> type;

    internal ExprAST[][]? initialValues;

    internal MultiDimArrayAST(
        string name,
        int rowSize,
        int colSize,
        PrimitiveType type,
        ExprAST[][]? initialValues,
        int lineNumber, int columnNumber

    ) : base(lineNumber, columnNumber) {

        this.name = name;
        this.rowSize = rowSize;
        this.colSize = colSize;
        this.type = new MultiDimArrayType<PrimitiveType>(type);
        
        this.initialValues = initialValues;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}