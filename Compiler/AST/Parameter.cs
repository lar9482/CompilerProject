using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

/** <identifier> : <SimpleType> **/
public sealed class ParameterAST : NodeAST {
    public readonly string name;
    public SimpleType type;
    
    public ParameterAST(
        string name, SimpleType type,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber){
        this.name = name;
        this.type = type;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}