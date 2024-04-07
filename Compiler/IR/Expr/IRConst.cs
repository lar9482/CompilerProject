
/** An intermediate representation for a 32-bit integer constant. CONST(n) */
public sealed class IRConst : IRExpr {
    public int value;

    public IRConst(int value) {
        this.value = value;
    }
}