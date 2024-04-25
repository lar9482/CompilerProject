using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class ArrayDeclCallAST : DeclAST {
    public readonly string name;
    public readonly FunctionCallAST function;
    public readonly ArrayType<PrimitiveType> declType;

    public ArrayDeclCallAST(
        string name,
        FunctionCallAST function,
        PrimitiveType declType,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.name = name;
        this.function = function;
        this.declType = new ArrayType<PrimitiveType>(declType);
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}