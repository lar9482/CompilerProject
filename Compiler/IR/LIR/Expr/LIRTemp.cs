
/** A lowered intermediate representation for a temporary register TEMP(name) **/
public sealed class LIRTemp : LIRExpr {
    public string name;
    
    public LIRTemp(string name) {
        this.name = name;
    }

    public override void accept(LIRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(LIRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}