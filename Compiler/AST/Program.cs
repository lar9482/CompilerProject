using CompilerProj.AST;
using CompilerProj.Visitors;
using CompilerProj.Context;

public sealed class ProgramAST : NodeAST {
    public List<DeclAST> declarations;
    public List<FunctionAST> functions;

    public SymbolTable? scope;
    
    public ProgramAST(
        List<DeclAST> declarations,
        List<FunctionAST> functions,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.declarations = declarations;
        this.functions = functions;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}