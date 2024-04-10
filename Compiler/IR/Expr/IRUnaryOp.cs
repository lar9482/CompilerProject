using CompilerProj.Visitors;

/** An intermediate representation for a binary operation UNARYOP(operand) */
public sealed class IRUnaryOp : IRExpr {
    public UnaryOpType opType;
    public IRExpr operand;

    public IRUnaryOp(UnaryOpType type, IRExpr operand) {
        this.opType = type;
        this.operand = operand;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}

public enum UnaryOpType {
    NOT,
    NEGATE
}