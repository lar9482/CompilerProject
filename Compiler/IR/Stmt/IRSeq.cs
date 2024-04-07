
/** An intermediate representation for a sequence of statements SEQ(s1,...,sn) */
public sealed class IRSeq : IRStmt {

    public List<IRStmt> statements;

    public IRSeq(List<IRStmt> statements) {
        this.statements = statements;
    }
}