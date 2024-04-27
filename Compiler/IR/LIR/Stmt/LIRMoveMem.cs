/** 
    A lowered intermediate representation for Move(dest, src),
    where dest is explicitly a MEM
**/
public sealed class LIRMoveMem : LIRStmt {
    public LIRExpr source;
    public LIRMem dest;
    
    public LIRMoveMem(LIRExpr source, LIRMem dest) {
        this.source = source;
        this.dest = dest;
    }
}