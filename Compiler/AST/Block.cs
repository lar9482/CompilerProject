using CompilerProj.AST;
using CompilerProj.Visitors;
using CompilerProj.Context;
using CompilerProj.Types;

/** { <StmtList> } **/
public sealed class BlockAST : NodeAST {
    public readonly List<StmtAST> statements;

    public SymbolTable? scope;
    public StmtType type;

    public BlockAST(
        List<StmtAST> statements, 
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.statements = statements;
        this.type = new UninitializedStmtType();
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}