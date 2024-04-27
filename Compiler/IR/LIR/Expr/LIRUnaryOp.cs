/** A lowered intermediate representation for a unary operation UnaryOp(operand) **/

public sealed class LIRUnaryOp : LIRExpr {
    public LUnaryOpType opType;
    public LIRExpr operand;

    public LIRUnaryOp(LIRExpr operand) {
        this.operand = operand;
    }
}

public enum LUnaryOpType {
    NOT,
    NEGATE
}