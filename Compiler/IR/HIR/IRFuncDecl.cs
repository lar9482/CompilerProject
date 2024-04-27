using CompilerProj.Visitors;
using CompilerProj.IR;

/** An IR function declaration */
public sealed class IRFuncDecl : IRNode {
    public string name;
    public IRStmt body;

    public IRFuncDecl(string name, IRStmt body) {
        this.name = name;
        this.body = body;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
}