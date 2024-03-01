using CompilerProj.AST;
using CompilerProj.Types;

public class FuncDecl : NodeAST {
    public string name;
    public List<ParameterAST> parameters;
    public List<LangType> returnTypes;
    
    public BlockAST block;

    public FuncDecl(
        string name,
        List<ParameterAST> parameters,
        List<LangType> returnTypes,
        BlockAST block,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.name = name;
        this.parameters = parameters;
        this.returnTypes = returnTypes;
        this.block = block;
    }
}