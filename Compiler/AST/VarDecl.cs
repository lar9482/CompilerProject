using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

internal sealed class VarDeclAST : NodeAST {
    internal string name;
    internal ExprAST? initialValue;
    internal PrimitiveType type;

    internal VarDeclAST(
        string name, 
        ExprAST? initialValue, 
        PrimitiveType type,
        int lineNumber, int columnNumber
        
    ) : base(lineNumber, columnNumber) {

        this.name = name;
        this.initialValue = initialValue;
        this.type = type;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}