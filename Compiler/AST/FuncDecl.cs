using CompilerProj.AST;
using CompilerProj.Types;
using CompilerProj.Visitors;

internal sealed class FuncDeclAST : NodeAST {
    internal string name;
    internal List<ParameterAST> parameters;
    internal List<SimpleType> returnTypes;
    
    internal BlockAST block;

    internal FuncDeclAST(
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