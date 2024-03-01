using CompilerProj.AST;
using CompilerProj.Types;

public class ParameterAST : NodeAST {
    public string name;
    public LangType type;
    
    public ParameterAST(
        string name, LangType type,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber){
        this.name = name;
        this.type = type;
    }
}