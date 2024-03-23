using CompilerProj.Visitors;

/*
 * This pass will
 */
public sealed class TypecheckerP1 : ASTVisitor {

    public void visit(ProgramAST program) { }
    public void visit(VarDeclAST varDecl) { }
    public void visit(MultiVarDeclAST multiVarDecl) { }
    public void visit(ArrayDeclAST array) { }
    public void visit(MultiDimArrayDeclAST multiDimArray) { }
    public void visit(FuncDeclAST function) { }
    public void visit(ParameterAST parameter) { }
    public void visit(BlockAST block) { }
    public void visit(ConditionalAST conditional) { }
    public void visit(WhileLoopAST whileLoop) { }


    //Unused visit procedures
    public void visit(AssignAST assign) { throw new NotImplementedException("This visit is not used"); }
    public void visit(MultiAssignAST multiAssign) { throw new NotImplementedException("This visit is not used"); }
    public void visit(MultiAssignCallAST multiAssignCall) { throw new NotImplementedException("This visit is not used"); }
    public void visit(ArrayAssignAST arrayAssign) { throw new NotImplementedException("This visit is not used"); }
    public void visit(MultiDimArrayAssignAST multiDimArrayAssign) { throw new NotImplementedException("This visit is not used"); }
    public void visit(ReturnAST returnStmt) { throw new NotImplementedException("This visit is not used"); }
    public void visit(FunctionCallAST functionCall) { throw new NotImplementedException("This visit is not used"); }
    public void visit(BinaryExprAST binaryExpr) { throw new NotImplementedException("This visit is not used"); }
    public void visit(UnaryExprAST unaryExpr) { throw new NotImplementedException("This visit is not used"); }
    public void visit(VarAccessAST varAccess) { throw new NotImplementedException("This visit is not used"); }
    public void visit(ArrayAccessAST arrayAccess) { throw new NotImplementedException("This visit is not used"); }
    public void visit(MultiDimArrayAccessAST multiDimArrayAccess) { throw new NotImplementedException("This visit is not used"); }
    public void visit(ProcedureCallAST procedureCall) { throw new NotImplementedException("This visit is not used"); }
    public void visit(IntLiteralAST intLiteral) { throw new NotImplementedException("This visit is not used"); }
    public void visit(BoolLiteralAST boolLiteral) { throw new NotImplementedException("This visit is not used"); }
    public void visit(CharLiteralAST charLiteral) { throw new NotImplementedException("This visit is not used"); }
    public void visit(StrLiteralAST strLiteral) { throw new NotImplementedException("This visit is not used"); }
}