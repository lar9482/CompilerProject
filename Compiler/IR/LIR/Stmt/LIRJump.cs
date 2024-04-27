/** 
    A lowered intermediate representation for JUMP(target)
**/
public sealed class LIRJump : LIRStmt {
    public LIRExpr target;
    
    public LIRJump(LIRExpr target) {
        this.target = target;
    }
}