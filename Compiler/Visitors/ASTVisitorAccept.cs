namespace CompilerProj.Visitors;

internal interface ASTVisitorAccept {
    public void accept(ASTVisitor visitor);
}