using CompilerProj.AST;

public class ProgramAST : NodeAST {
    public List<VarDecl>? varDecls;
    public List<MultiVarDecl>? multiVarDecls;
    public List<FuncDecl> funcDecls;

    public ProgramAST(
        List<VarDecl>? varDecls, 
        List<MultiVarDecl>? multiVarDecls,
        List<FuncDecl> funcDecls,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.varDecls = varDecls;
        this.multiVarDecls = multiVarDecls;
        this.funcDecls = funcDecls;
    }
}