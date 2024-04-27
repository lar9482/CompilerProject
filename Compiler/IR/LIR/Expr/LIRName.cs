
/** A lowered intermediate representation for a labeled address in memory **/
public sealed class LIRName : LIRExpr {
    public string name;

    public LIRName(string name) {
        this.name = name;
    }
}