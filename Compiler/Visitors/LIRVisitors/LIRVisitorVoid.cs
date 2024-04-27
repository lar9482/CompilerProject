public interface LIRVisitorVoid {

    public void visit(LIRCompUnit compUnit);
    public void visit(LIRFuncDecl funcDecl);

    public void visit(LIRMoveTemp moveTemp);
    public void visit(LIRMoveMem moveMem);
    public void visit(LIRJump jump);
    public void visit(LIRCJump cJump);
    public void visit(LIRReturn Return);
    public void visit(LIRLabel label);
    public void visit(LIRCallM callM);

    public void visit(LIRBinOp binOp);
    public void visit(LIRUnaryOp unaryOp);
    public void visit(LIRMem mem);
    public void visit(LIRTemp temp);
    public void visit(LIRName name);
    public void visit(LIRConst Const);
}