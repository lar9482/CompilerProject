/** 
    A lowered intermediate representation for JUMP(target)
**/
public sealed class LIRJump : LIRStmt {
    public LIRExpr target;
    
    public LIRJump(LIRExpr target) {
        this.target = target;
    }

    public override void accept(LIRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(LIRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}