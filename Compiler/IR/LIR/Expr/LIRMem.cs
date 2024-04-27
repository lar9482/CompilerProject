
/** A lowered intermediate representation for a memory operation MEM(address) **/
public sealed class LIRMem : LIRExpr {
    public LIRExpr address;

    public LIRMem(LIRExpr address) {
        this.address = address;
    }
}