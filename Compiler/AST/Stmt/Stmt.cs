using CompilerProj.AST;
using CompilerProj.Types;

public abstract class StmtAST : NodeAST {
    public StmtType type;

    public StmtAST(int lineNumber, int columnNumber) : base(lineNumber, columnNumber) {
        this.type = new UninitializedStmtType();
    }
}