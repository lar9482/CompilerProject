using CompilerProj.AST;

public abstract class DeclAST : StmtAST {
    public DeclAST(int lineNumber, int columnNumber) : base(lineNumber, columnNumber) { }
}