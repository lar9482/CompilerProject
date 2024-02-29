using CompilerProj.AST;

public abstract class ExprAST : NodeAST {
    
    public ExprAST(int lineNumber, int columnNumber) : base(lineNumber, columnNumber) {

    }
}