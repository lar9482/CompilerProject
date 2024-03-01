using CompilerProj.AST;
using CompilerProj.Types;

public class ArrayAST : NodeAST {
    public string name;
    public int size;
    public ArrayType<PrimitiveType> type;
    public ExprAST[] initialValues;

    public ArrayAST(
        string name,
        int size,
        PrimitiveType type,
        ExprAST[] initialValues,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.name = name;
        this.size = size;
        this.type = new ArrayType<PrimitiveType>(type);
        this.initialValues = initialValues;
    }
}