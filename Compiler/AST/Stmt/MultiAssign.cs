using CompilerProj.Visitors;

/** <variable1>, <variable2>, ... ,<variableN> = <Expr1>, <Expr2>, ... , <ExprN> **/
public sealed class MultiAssignAST : StmtAST{

    public readonly Dictionary<VarAccessAST, ExprAST> assignments;

    public MultiAssignAST (
        Dictionary<VarAccessAST, ExprAST> assignments,
        int lineNumber, int columnNumber
    ) : base(lineNumber, columnNumber) {
        
        this.assignments = assignments;
    }

    public override void accept(ASTVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(ASTVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}