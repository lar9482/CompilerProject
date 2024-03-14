using CompilerProj.Visitors;

namespace CompilerProj.AST;

internal abstract class NodeAST : ASTVisitorAccept {
    internal int lineNumber;
    internal int columnNumber;

    internal NodeAST(int lineNumber, int columnNumber) {
        this.lineNumber = lineNumber;
        this.columnNumber = columnNumber;
    }

    public abstract void accept(ASTVisitor visitor);
}