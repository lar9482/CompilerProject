using CompilerProj.Visitors;
using CompilerProj.IR;

/** An lowered IR function declaration */
public sealed class LIRFuncDecl : LIRNode {
    public string name;
    public List<LIRStmt> body;

    public LIRFuncDecl(string name, List<LIRStmt> body) {
        this.name = name;
        this.body = body;
    }

    public override void accept(LIRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(LIRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}