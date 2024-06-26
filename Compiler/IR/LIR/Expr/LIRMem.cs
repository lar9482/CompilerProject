
/** A lowered intermediate representation for a memory operation MEM(address) **/
public sealed class LIRMem : LIRExpr {
    public LIRExpr address;

    public LIRMem(LIRExpr address) {
        this.address = address;
    }

    public override void accept(LIRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(LIRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}