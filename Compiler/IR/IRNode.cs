using CompilerProj.Visitors;

namespace CompilerProj.IR;

public abstract class IRNode : IRVisitorVoidAccept, IRVisitorGenericAccept {

    public abstract void accept(IRVisitorVoid visitor);
    public abstract T accept<T>(IRVisitorGeneric visitor);
}