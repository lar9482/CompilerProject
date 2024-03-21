using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class ParameterAST : NodeAST {
    public string name;
    public SimpleType type;
    
    public ParameterAST(
        string name, SimpleType type,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber){
        this.name = name;
        this.type = type;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}