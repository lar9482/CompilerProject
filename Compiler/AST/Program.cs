using CompilerProj.AST;
using CompilerProj.Visitors;
using CompilerProj.Context;

public sealed class ProgramAST : NodeAST {
    public List<VarDeclAST> varDecls;
    public List<MultiVarDeclAST> multiVarDecls;
    public List<ArrayAST> arrayDecls;
    public List<MultiDimArrayAST> multiDimArrayDecls;

    public List<FuncDeclAST> funcDecls;

    public SymbolTable? scope;
    
    public ProgramAST(
        List<VarDeclAST> varDecls, 
        List<MultiVarDeclAST> multiVarDecls,
        List<ArrayAST> arrayDecls,
        List<MultiDimArrayAST> multiDimArrayDecls,
        List<FuncDeclAST> funcDecls,
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