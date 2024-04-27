/** 
    A lowered intermediate representation for LABEL(label)
**/

public sealed class LIRLabel : LIRStmt {
    public string label;

    public LIRLabel(string label) {
        this.label = label;
    }

    public override void accept(LIRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(LIRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}