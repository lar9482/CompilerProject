using CompilerProj.IR;

/** An intermediate representation for a compilation unit */
public sealed class IRCompUnit : IRNode {
    public readonly string name;
    public readonly Dictionary<String, IRFuncDecl> functions;
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
} 