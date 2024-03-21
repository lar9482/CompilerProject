using CompilerProj.AST;
using CompilerProj.Visitors;

public sealed class BlockAST : NodeAST {
    public List<VarDeclAST> varDecls;
    public List<MultiVarDeclAST> multiVarDecls;
    public List<ArrayAST> arrays;
    public List<MultiDimArrayAST> multiDimArrays;

    public List<StmtAST> statements;

    public SymbolTable? scope;

    public BlockAST(
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