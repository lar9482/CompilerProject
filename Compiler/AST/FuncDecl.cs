using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

public sealed class FuncDeclAST : NodeAST {
    public string name;
    public List<ParameterAST> parameters;
    public List<SimpleType> returnTypes;
    
    public BlockAST block;

    public SymbolTable? scope;
    
    public FuncDeclAST(
        string name,
        List<ParameterAST> parameters,
        List<SimpleType> returnTypes,
        BlockAST block,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {

        this.name = name;
        this.parameters = parameters;
        this.returnTypes = returnTypes;
        this.block = block;
    }

    public override void accept(ASTVisitor visitor) {
        visitor.visit(this);
    }
}