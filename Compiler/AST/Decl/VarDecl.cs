using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class VarDeclAST : DeclAST {
    public string name;
    public ExprAST? initialValue;
    public PrimitiveType type;

    public VarDeclAST(
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