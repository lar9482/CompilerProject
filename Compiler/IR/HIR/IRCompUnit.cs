using CompilerProj.IR;
using CompilerProj.Visitors;

/** An intermediate representation for a compilation unit */
public sealed class IRCompUnit : IRNode {
    public readonly string name;
    public readonly Dictionary<string, IRFuncDecl> functions;
    public readonly List<string> ctors;
    
    public IRCompUnit(
        string name,
        Dictionary<String, IRFuncDecl> functions,
        List<string> ctors
    ) {
        this.name = name;
        this.functions = functions;
        this.ctors = ctors;
    }

    public override void accept(IRVisitorVoid visitor) {
        visitor.visit(this);
    }

    public override T accept<T>(IRVisitorGeneric visitor) {
        return visitor.visit<T>(this);
    }
} 