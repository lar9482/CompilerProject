
/** A lowered intermediate representation for a const CONST(n) **/
public sealed class LIRConst : LIRExpr {
    public int value;

    public LIRConst(int value) {
        this.value = value;
    }
}