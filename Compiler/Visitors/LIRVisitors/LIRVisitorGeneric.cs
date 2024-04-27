public interface LIRVisitorGeneric {

    public T visit<T>(LIRCompUnit compUnit);
    public T visit<T>(LIRFuncDecl funcDecl);

    public T visit<T>(LIRMoveTemp moveTemp);
    public T visit<T>(LIRMoveMem moveMem);
    public T visit<T>(LIRJump jump);
    public T visit<T>(LIRCJump cJump);
    public T visit<T>(LIRReturn Return);
    public T visit<T>(LIRLabel label);
    public T visit<T>(LIRCallM callM);

    public T visit<T>(LIRBinOp binOp);
    public T visit<T>(LIRUnaryOp unaryOp);
    public T visit<T>(LIRMem mem);
    public T visit<T>(LIRTemp temp);
    public T visit<T>(LIRName name);
    public T visit<T>(LIRConst Const);
}