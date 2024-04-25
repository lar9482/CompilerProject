using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class MultiDimArrayDeclCallAST : DeclAST {
    public readonly string name;
    public readonly FunctionCallAST function;
    public readonly MultiDimArrayType<PrimitiveType> declType;

    public MultiDimArrayDeclCallAST(
        string name,
        FunctionCallAST function,
        PrimitiveType declType,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.name = name;
        this.function = function;
        this.declType = new MultiDimArrayType<PrimitiveType>(declType);
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}