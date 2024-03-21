using CompilerProj.AST;
using CompilerProj.Visitors;

internal sealed class BlockAST : NodeAST {
    internal List<VarDeclAST> varDecls;
    internal List<MultiVarDeclAST> multiVarDecls;
    internal List<ArrayAST> arrays;
    internal List<MultiDimArrayAST> multiDimArrays;

    internal List<StmtAST> statements;

    internal SymbolTable? scope;

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

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}