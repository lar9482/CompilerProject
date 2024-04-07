
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
}