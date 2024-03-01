using CompilerProj.AST;
using CompilerProj.Types;

public class MultiVarDecl : NodeAST {
    public List<string> names;
    public List<ExprAST>? initialValues;
    public List<PrimitiveType> types;

    public MultiVarDecl(
        List<string> names, 
        List<ExprAST>? initialValues, 
        List<PrimitiveType> types,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.names = names;
        this.initialValues = initialValues;
        this.types = types;
    }
}