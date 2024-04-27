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

    public override void accept(LIRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(LIRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}