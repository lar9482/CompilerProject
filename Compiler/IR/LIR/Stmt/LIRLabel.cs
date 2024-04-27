/** 
    A lowered intermediate representation for LABEL(label)
**/

public sealed class LIRLabel : LIRStmt {
    public string label;

    public LIRLabel(string label) {
        this.label = label;
    }
}