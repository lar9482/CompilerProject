using CompilerProj.AST;

internal abstract class ExprAST : NodeAST {
    
    internal ExprAST(int lineNumber, int columnNumber) : base(lineNumber, columnNumber) {

    }
}