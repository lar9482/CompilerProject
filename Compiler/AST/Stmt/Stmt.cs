using CompilerProj.AST;
using CompilerProj.Types;

internal abstract class StmtAST : NodeAST {
    internal StmtType? type;

    internal StmtAST(int lineNumber, int columnNumber) : base(lineNumber, columnNumber) {

    }
}