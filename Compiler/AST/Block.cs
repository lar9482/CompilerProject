using CompilerProj.AST;
using CompilerProj.Visitors;
using CompilerProj.Context;
using CompilerProj.Types;

public sealed class BlockAST : NodeAST {
    public List<StmtAST> statements;

    public SymbolTable? scope;

    public StmtType type;

    public BlockAST(
        List<StmtAST> statements, 
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.statements = statements;
        this.type = new UninitializedStmtType();
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}