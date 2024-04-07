using CompilerProj.AST;
using CompilerProj.Visitors;
using CompilerProj.Context;

public sealed class ProgramAST : NodeAST {
    public readonly List<DeclAST> declarations;
    public readonly List<FunctionAST> functions;

    public SymbolTable? scope;
    
    public ProgramAST(
        List<DeclAST> declarations,
        List<FunctionAST> functions,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.declarations = declarations;
        this.functions = functions;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}