using CompilerProj.Visitors;

/**
 * An intermediate representation for a conditional transfer of control CJUMP(expr, trueLabel,
 * falseLabel)
 */
public sealed class IRCJump : IRStmt {
    public IRExpr cond;
    public string trueLabel, falseLabel;

    public IRCJump(IRExpr cond, string trueLabel, string falseLabel) {
        this.cond = cond;
        this.trueLabel = trueLabel;
        this.falseLabel = falseLabel;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}