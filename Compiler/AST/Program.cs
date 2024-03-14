using CompilerProj.AST;
using CompilerProj.Visitors;

internal sealed class ProgramAST : NodeAST {
    internal List<VarDeclAST> varDecls;
    internal List<MultiVarDeclAST> multiVarDecls;
    internal List<ArrayAST> arrayDecls;
    internal List<MultiDimArrayAST> multiDimArrayDecls;

    internal List<FuncDeclAST> funcDecls;

    internal ProgramAST(
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