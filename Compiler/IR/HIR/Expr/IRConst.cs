using CompilerProj.Visitors;

/** An intermediate representation for a 32-bit integer constant. CONST(n) */
public sealed class IRConst : IRExpr {
    public int value;

    public IRConst(int value) {
        this.value = value;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}