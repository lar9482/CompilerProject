namespace CompilerProj.AST;

public abstract class NodeAST {
    public int lineNumber;
    public int columnNumber;

    public NodeAST(int lineNumber, int columnNumber) {
        this.lineNumber = lineNumber;
        this.columnNumber = columnNumber;
    }
}