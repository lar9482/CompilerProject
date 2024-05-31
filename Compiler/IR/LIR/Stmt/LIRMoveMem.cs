/** 
    A lowered intermediate representation for Move(dest, src),
    where dest is explicitly a MEM
**/
public sealed class LIRMoveMem : LIRStmt {
    public LIRExpr source;
    public LIRMem dest;
    
    public LIRMoveMem(LIRMem dest, LIRExpr source) {
        this.source = source;
        this.dest = dest;
    }

    public override void accept(LIRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(LIRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}