using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

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

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}