using CompilerProj.AST;
using CompilerProj.Types;

internal sealed class ParameterAST : NodeAST {
    internal string name;
    internal LangType type;
    
    internal ParameterAST(
        string name, LangType type,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber){
        this.name = name;
        this.type = type;
    }
}