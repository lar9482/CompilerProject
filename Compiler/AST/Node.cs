using CompilerProj.Visitors;

namespace CompilerProj.AST;

public abstract class NodeAST : ASTVisitorVoidAccept, ASTVisitorGenericAccept {
    public int lineNumber;
    public int columnNumber;

    public NodeAST(int lineNumber, int columnNumber) {
        this.lineNumber = lineNumber;
        this.columnNumber = columnNumber;
    }

    public abstract void accept(ASTVisitorVoid visitor);
    public abstract T accept<T>(ASTVisitorGeneric visitor);
}