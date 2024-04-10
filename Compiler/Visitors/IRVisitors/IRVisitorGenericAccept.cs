namespace CompilerProj.Visitors;

public interface IRVisitorGenericAccept {
    public T accept<T>(IRVisitorGeneric visitor);
}