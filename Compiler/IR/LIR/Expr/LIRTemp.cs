
/** A lowered intermediate representation for a temporary register TEMP(name) **/
public sealed class LIRTemp : LIRExpr {
    public string name;
    
    public LIRTemp(string name) {
        this.name = name;
    }
}