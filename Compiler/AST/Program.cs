using CompilerProj.AST;

public class ProgramAST : NodeAST {
    public List<VarDeclAST> varDecls;
    public List<MultiVarDeclAST> multiVarDecls;
    public List<ArrayAST> arrayDecls;
    public List<MultiDimArrayAST> multiDimArrayDecls;

    public List<FuncDecl> funcDecls;

    public ProgramAST(
        List<VarDeclAST> varDecls, 
        List<MultiVarDeclAST> multiVarDecls,
        List<ArrayAST> arrayDecls,
        List<MultiDimArrayAST> multiDimArrayDecls,
        List<FuncDecl> funcDecls,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.varDecls = varDecls;
        this.multiVarDecls = multiVarDecls;
        this.arrayDecls = arrayDecls;
        this.multiDimArrayDecls = multiDimArrayDecls;
        this.funcDecls = funcDecls;
    }
}