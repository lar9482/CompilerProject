using CompilerProj.AST;
using CompilerProj.Types;

public class MultiDimArrayAST : NodeAST {
    public string name;
    public int rowSize;
    public int colSize;
    public MultiDimArrayType<PrimitiveType> type;

    public ExprAST[][] initialValues;

    public MultiDimArrayAST(
        string name,
        int rowSize,
        int colSize,
        PrimitiveType type,
        ExprAST[][] initialValues,
        int lineNumber, int columnNumber

    ) : base(lineNumber, columnNumber) {

        this.name = name;
        this.rowSize = rowSize;
        this.colSize = colSize;
        this.type = new MultiDimArrayType<PrimitiveType>(type);
        
        this.initialValues = initialValues;
    }
}