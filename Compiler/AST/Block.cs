using CompilerProj.AST;

internal sealed class BlockAST : NodeAST {
    internal List<VarDeclAST> varDecls;
    internal List<MultiVarDeclAST> multiVarDecls;
    internal List<ArrayAST> arrays;
    internal List<MultiDimArrayAST> multiDimArrays;

    internal List<StmtAST> statements;

    internal BlockAST(
        List<VarDeclAST> varDecls,
        List<MultiVarDeclAST> multiVarDecls,
        List<ArrayAST> arrays,
        List<MultiDimArrayAST> multiDimArrays,
        List<StmtAST> statements, 
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.varDecls = varDecls;
        this.multiVarDecls = multiVarDecls;
        this.arrays = arrays;
        this.multiDimArrays = multiDimArrays;
        this.statements = statements;
    }
}