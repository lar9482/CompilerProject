/** A lowered intermediate representation for a unary operation UnaryOp(operand) **/

public sealed class LIRUnaryOp : LIRExpr {
    public LUnaryOpType opType;
    public LIRExpr operand;

    public LIRUnaryOp(LIRExpr operand) {
        this.operand = operand;
    }

    public override void accept(LIRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(LIRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}

public enum LUnaryOpType {
    NOT,
    NEGATE
}