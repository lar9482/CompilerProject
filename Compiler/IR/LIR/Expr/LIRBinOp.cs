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