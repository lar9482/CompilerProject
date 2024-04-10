using CompilerProj.Visitors;

/** An intermediate representation for a sequence of statements SEQ(s1,...,sn) */
public sealed class IRSeq : IRStmt {

    public List<IRStmt> statements;

    public IRSeq(List<IRStmt> statements) {
        this.statements = statements;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}