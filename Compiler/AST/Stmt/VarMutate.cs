using CompilerProj.Visitors;

/** Either <variable>++ or <variable>-- **/
public sealed class VarMutateAST : StmtAST { 
    public readonly VarAccessAST variable;
    public readonly bool increment;

    public VarMutateAST(
        VarAccessAST variable, 
        bool increment,
        int lineNumber,
        int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.variable = variable;
        this.increment = increment;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}