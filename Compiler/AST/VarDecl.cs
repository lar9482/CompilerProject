using CompilerProj.AST;
using CompilerProj.Types;

public class VarDecl : NodeAST {
    public string name;
    public ExprAST? initialValue;
    public PrimitiveType type;

    public VarDecl(
        string name, 
        ExprAST? initialValue, 
        PrimitiveType type,
        int lineNumber, int columnNumber
        
    ) : base(lineNumber, columnNumber) {

        this.name = name;
        this.initialValue = initialValue;
        this.type = type;
    }
}