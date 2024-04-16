using CompilerProj.Visitors;

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

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}

public enum BinOpType {
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