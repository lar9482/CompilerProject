
/** An intermediate representation for a binary operation UNARYOP(operand) */
public sealed class IRUnaryOp : IRExpr {
    public UnaryOpType opType;
    public IRExpr operand;

    public IRUnaryOp(UnaryOpType type, IRExpr operand) {
        this.opType = type;
        this.operand = operand;
    }
}

public enum UnaryOpType {
    NOT,
    NEGATE
}