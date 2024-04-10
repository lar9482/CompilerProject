namespace CompilerProj.Visitors;

public interface IRVisitorGeneric {
    public T visit<T>(IRCompUnit compUnit);
    public T visit<T>(IRFuncDecl funcDecl);

    //Stmt IR
    public T visit<T>(IRMove move);
    public T visit<T>(IRSeq seq);
    public T visit<T>(IR_Eseq Eseq);
    public T visit<T>(IRCJump cJump);
    public T visit<T>(IRExp exp);
    public T visit<T>(IRJump jump);
    public T visit<T>(IRLabel label);
    public T visit<T>(IRReturn Return);
    public T visit<T>(IRCallStmt callStmt);

    //Expr IR
    public T visit<T>(IRBinOp binOp);
    public T visit<T>(IRUnaryOp unaryOp);
    public T visit<T>(IRCall call);
    public T visit<T>(IRConst Const);
    public T visit<T>(IRMem mem);
    public T visit<T>(IRName name);
    public T visit<T>(IRTemp temp);
}