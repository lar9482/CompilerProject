using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class MultiDimArrayDeclCallAST : DeclAST {
    public readonly string name;
    public readonly string functionName;
    public readonly List<ExprAST> args;
    public readonly MultiDimArrayType<PrimitiveType> declType;

    public MultiDimArrayDeclCallAST(
        string name,
        string functionName,
        List<ExprAST> args,
        PrimitiveType declType,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.name = name;
        this.functionName = functionName;
        this.args = args;
        this.declType = new MultiDimArrayType<PrimitiveType>(declType);
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}