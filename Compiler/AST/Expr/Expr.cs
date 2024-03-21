using CompilerProj.AST;

internal abstract class ExprAST : NodeAST {
    
    internal SimpleType? type;
    
    internal ExprAST(int lineNumber, int columnNumber) : base(lineNumber, columnNumber) {
    }
}