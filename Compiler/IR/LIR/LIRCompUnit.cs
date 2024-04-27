using CompilerProj.IR;
using CompilerProj.Visitors;

/** An lowered intermediate representation for a compilation unit */
public sealed class LIRCompUnit : LIRNode {
    public readonly string name;
    public readonly Dictionary<string, LIRFuncDecl> functions;

    public LIRCompUnit(string name, Dictionary<string, LIRFuncDecl> functions) {
        this.name = name;
        this.functions = functions;
    }

    public override void accept(LIRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(LIRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}