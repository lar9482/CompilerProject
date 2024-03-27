using CompilerProj.AST;
using CompilerProj.Visitors;
using CompilerProj.Context;

public sealed class BlockAST : NodeAST {
    public List<StmtAST> statements;

    public SymbolTable? scope;

    public BlockAST(
        List<StmtAST> statements, 
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.statements = statements;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}