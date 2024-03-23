using CompilerProj.AST;
using CompilerProj.Visitors;
using CompilerProj.Context;

public sealed class BlockAST : NodeAST {
    public List<DeclAST> declarations;
    public List<StmtAST> statements;

    public SymbolTable? scope;

    public BlockAST(
        List<DeclAST> declarations,
        List<StmtAST> statements, 
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.declarations = declarations;
        this.statements = statements;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}