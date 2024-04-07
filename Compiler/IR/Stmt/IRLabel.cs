
/** An intermediate representation for naming a memory address */
public sealed class IRLabel : IRStmt {
    public string name;

    public IRLabel(string name) {
        this.name = name;
    }
}