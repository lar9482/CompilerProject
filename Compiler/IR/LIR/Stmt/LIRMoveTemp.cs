/** 
    A lowered intermediate representation for Move(dest, src),
    where dest is explicitly a TEMP
**/
public sealed class LIRMoveTemp : LIRStmt {
    public LIRExpr source;
    public LIRTemp dest;

    public LIRMoveTemp(LIRExpr source, LIRTemp dest) {
        this.source = source;
        this.dest = dest;
    }
}