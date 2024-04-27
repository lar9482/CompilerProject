/** 
    A lowered intermediate representation for CJump(condition, t, f),
**/
public sealed class LIRCJump : LIRStmt {
    public LIRExpr cond;
    public string trueLabel, falseLabel;

    public LIRCJump(LIRExpr cond, string trueLabel, string falseLabel) {
        this.cond = cond;
        this.trueLabel = trueLabel;
        this.falseLabel = falseLabel;
    }

    public override void accept(LIRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(LIRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}