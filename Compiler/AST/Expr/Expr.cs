using CompilerProj.AST;

public abstract class ExprAST : NodeAST {
    
    public SimpleType type;
    
    public ExprAST(int lineNumber, int columnNumber) : base(lineNumber, columnNumber) {
        this.type = new UninitializedType();
    }
}