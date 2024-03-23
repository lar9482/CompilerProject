
namespace CompilerProj.Context;

public abstract class Symbol {
    public string identifier;

    public Symbol(string identifier) {
        this.identifier = identifier;
    }
}

public sealed class SymbolVariable : Symbol {
    public SimpleType type;

    public SymbolVariable(string variableName, SimpleType type) : base(variableName) {
        this.type = type;
    }
}

public sealed class SymbolFunction : Symbol {
    public SimpleType[] parameterTypes;
    public SimpleType[] returnTypes;

    public SymbolFunction(
        string functionName,
        SimpleType[] parameterTypes,
        SimpleType[] returnTypes
    ) : base(functionName) {
        this.parameterTypes = parameterTypes;
        this.returnTypes = returnTypes;
    }
}