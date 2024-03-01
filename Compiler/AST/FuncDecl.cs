using CompilerProj.AST;
using CompilerProj.Types;

public class FuncDecl : NodeAST {
    public string name;
    public List<ParameterAST> parameters;
    public List<LangType> returnTypes;
    public List<StmtAST> statements;

    public FuncDecl(
        string name,
        List<ParameterAST> parameters,
        List<LangType> returnTypes,
        List<StmtAST> statements,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.name = name;
        this.parameters = parameters;
        this.returnTypes = returnTypes;
        this.statements = statements;
    }
}