using CompilerProj.AST;

internal abstract class StmtAST : NodeAST {
    
    internal StmtAST(int lineNumber, int columnNumber) : base(lineNumber, columnNumber) {

    }
}