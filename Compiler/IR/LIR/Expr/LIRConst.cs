
/** A lowered intermediate representation for a const CONST(n) **/
public sealed class LIRConst : LIRExpr {
    public int value;

    public LIRConst(int value) {
        this.value = value;
    }

    public override void accept(LIRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(LIRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}