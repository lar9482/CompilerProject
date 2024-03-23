using CompilerProj.AST;
using CompilerProj.Visitors;
using CompilerProj.Context;

public sealed class ProgramAST : NodeAST {
    public List<VarDeclAST> varDecls;
    public List<MultiVarDeclAST> multiVarDecls;
    public List<ArrayDeclAST> arrayDecls;
    public List<MultiDimArrayDeclAST> multiDimArrayDecls;

    public List<FunctionAST> funcDecls;

    public SymbolTable? scope;
    
    public ProgramAST(
        List<VarDeclAST> varDecls, 
        List<MultiVarDeclAST> multiVarDecls,
        List<ArrayDeclAST> arrayDecls,
        List<MultiDimArrayDeclAST> multiDimArrayDecls,
        List<FunctionAST> funcDecls,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.varDecls = varDecls;
        this.multiVarDecls = multiVarDecls;
        this.arrayDecls = arrayDecls;
        this.multiDimArrayDecls = multiDimArrayDecls;
        this.funcDecls = funcDecls;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}