using CompilerProj.AST;

public abstract class DeclAST : NodeAST {
    public DeclAST(int lineNumber, int columnNumber) : base(lineNumber, columnNumber) { }
}