using CompilerProj.AST;
using CompilerProj.Types;

public class MultiVarDeclAST : NodeAST {
    public List<string> names;
    public Dictionary<string, ExprAST?> initialValues;
    public Dictionary<string, PrimitiveType> types;

    public MultiVarDeclAST(
        List<string> names, 
        Dictionary<string, ExprAST?> initialValues,
        Dictionary<string, PrimitiveType> types,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.names = names;
        this.initialValues = initialValues;
        this.types = types;
    }
}