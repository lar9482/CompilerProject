using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class MultiDimArrayAST : NodeAST {
    public string name;
    public ExprAST? rowSize;
    public ExprAST? colSize;
    public MultiDimArrayType<PrimitiveType> type;

    public ExprAST[][]? initialValues;

    public MultiDimArrayAST(
        string name,
        ExprAST? rowSize,
        ExprAST? colSize,
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