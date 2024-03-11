namespace CompilerProj.AST;

internal abstract class NodeAST {
    internal int lineNumber;
    internal int columnNumber;

    internal NodeAST(int lineNumber, int columnNumber) {
        this.lineNumber = lineNumber;
        this.columnNumber = columnNumber;
    }
}