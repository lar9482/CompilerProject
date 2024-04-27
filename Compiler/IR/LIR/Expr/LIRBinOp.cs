/** A lowered intermediate representation for a binary operation LBinOp(left, right) **/
public sealed class LIRBinOp : LIRExpr {
    public LBinOpType opType;
    public LIRExpr left;
    public LIRExpr right;

    public LIRBinOp(LBinOpType opType, LIRExpr left, LIRExpr right) {
        this.opType = opType;
        this.left = left;
        this.right = right;
    }

    public override void accept(LIRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(LIRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}

public enum LBinOpType {
    ADD,
    SUB,
    MUL,
    DIV,
    MOD,
    AND,
    OR,

    XOR,
    LSHIFT,
    RSHIFT,

    EQ,
    NEQ,
    LT,
    GT,
    LEQ,
    GEQ,
    
    ULT
}