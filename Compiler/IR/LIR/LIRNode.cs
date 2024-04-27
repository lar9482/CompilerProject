using CompilerProj.Visitors;

public abstract class LIRNode : LIRVisitorVoidAccept, LIRVisitorGenericAccept {
    public abstract void accept(LIRVisitorVoid visitor);
    public abstract T accept<T>(LIRVisitorGeneric visitor);
}