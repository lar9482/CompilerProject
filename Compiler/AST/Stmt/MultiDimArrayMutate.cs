using CompilerProj.Visitors;

/** <arrayName>[<Expr>][<Expr>]++ or <arrayName>[<Expr>][<Expr>]--**/
public sealed class MultiDimArrayMutateAST : StmtAST {
    public readonly MultiDimArrayAccessAST arrayAccess;
    public readonly bool increment;

    public MultiDimArrayMutateAST(
        MultiDimArrayAccessAST arrayAccess,
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