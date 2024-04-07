using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;
using CompilerProj.Context;

public sealed class FunctionAST : NodeAST {
    public readonly string name;
    public readonly List<ParameterAST> parameters;
    public readonly List<SimpleType> returnTypes; 
    public readonly BlockAST block;

    public SymbolTable? scope;
    
    public FunctionAST(
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

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}