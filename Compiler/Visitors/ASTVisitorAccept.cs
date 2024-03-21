namespace CompilerProj.Visitors;

public interface ASTVisitorAccept {
    public void accept(ASTVisitor visitor);
}