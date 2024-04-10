namespace CompilerProj.Visitors;

public interface IRVisitorVoid {
    public void visit(IRCompUnit compUnit);
    public void visit(IRFuncDecl funcDecl);

    //Stmt IR
    public void visit(IRMove move);
    public void visit(IRSeq seq);
    public void visit(IR_Eseq Eseq);
    public void visit(IRCJump cJump);
    public void visit(IRExp exp);
    public void visit(IRJump jump);
    public void visit(IRLabel label);
    public void visit(IRReturn Return);
    public void visit(IRCallStmt callStmt);

    //Expr IR
    public void visit(IRBinOp binOp);
    public void visit(IRUnaryOp unaryOp);
    public void visit(IRCall call);
    public void visit(IRConst Const);
    public void visit(IRMem mem);
    public void visit(IRName name);
    public void visit(IRTemp temp);
}