using CompilerProj.Visitors;

/** An intermediate representation for a memory location MEM(e) */
public sealed class IRMem : IRExpr {
    public MemType memType;
    public IRExpr expr;

    public IRMem(MemType memType, IRExpr expr) {
        this.memType = memType;
        this.expr = expr;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}

public enum MemType {
    NORMAL,
    IMMUTABLE
}