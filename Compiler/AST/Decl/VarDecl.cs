using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class VarDeclAST : DeclAST {
    public string name;
    public ExprAST? initialValue;
    public PrimitiveType declType;

    public VarDeclAST(
        string name, 
        ExprAST? initialValue, 
        PrimitiveType declType,
        int lineNumber, int columnNumber
        
    ) : base(lineNumber, columnNumber) {

        this.name = name;
        this.initialValue = initialValue;
        this.declType = declType;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}