namespace CompilerProj.Visitors;

public interface LIRVisitorGenericAccept {
    public T accept<T>(LIRVisitorGeneric visitor);
}