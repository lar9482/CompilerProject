
/** A lowered intermediate representation for a labeled address in memory **/
public sealed class LIRName : LIRExpr {
    public string name;

    public LIRName(string name) {
        this.name = name;
    }

    public override void accept(LIRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(LIRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}