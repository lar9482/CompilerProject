using CompilerProj.Visitors;

/** Either <arrayName>[ <Expr> ]++ or <arrayName>[ <Expr> ]-**/
public sealed class ArrayMutateAST : StmtAST {
    public readonly ArrayAccessAST arrayAccess;
    public readonly bool increment;

    public ArrayMutateAST(
        ArrayAccessAST arrayAccess, 
        bool increment,
        int lineNumber, 
        int columnNumber
    ) : base(lineNumber, columnNumber) {
        this.arrayAccess = arrayAccess;
        this.increment = increment;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}