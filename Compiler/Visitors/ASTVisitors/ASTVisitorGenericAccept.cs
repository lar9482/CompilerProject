namespace CompilerProj.Visitors;

public interface ASTVisitorGenericAccept {
    public T accept<T>(ASTVisitorGeneric visitor);
}