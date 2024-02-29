using CompilerProj.AST;

public abstract class StmtAST : NodeAST {
    
    public StmtAST(int lineNumber, int columnNumber) : base(lineNumber, columnNumber) {

    }
}