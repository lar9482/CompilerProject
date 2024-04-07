
/** An intermediate representation for a memory location MEM(e) */
public sealed class IRMem : IRExpr {
    public MemType memType;
    public IRExpr expr;

    public IRMem(MemType memType, IRExpr expr) {
        this.memType = memType;
        this.expr = expr;
    }
}

public enum MemType {
    NORMAL,
    IMMUTABLE
}