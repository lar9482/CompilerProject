
/** An intermediate representation for a temporary register TEMP(name) */
public sealed class IRTemp : IRExpr {
    public string name;

    public IRTemp(string name) {
        this.name = name;
    }
}