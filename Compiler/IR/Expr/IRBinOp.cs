
/** An intermediate representation for a binary operation BINOP(left, right) */
public sealed class IRBinOp : IRExpr {
    public BinOpType opType;
    public IRExpr left;
    public IRExpr right;

    public IRBinOp(BinOpType type, IRExpr left, IRExpr right) {
        this.opType = type;
        this.left = left;
        this.right = right;
    }
}

public enum BinOpType {
    ADD,
    SUB,
    MUL,
    HMUL,
    DIV,
    MOD,
    AND,
    OR,
    XOR,
    LSHIFT,
    RSHIFT,
    ARSHIFT,
    EQ,
    NEQ,
    LT,
    ULT,
    GT,
    LEQ,
    GEQ
}