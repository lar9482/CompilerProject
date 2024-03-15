using CompilerProj.Visitors;

internal sealed class ASTVisitorImplemented : ASTVisitor {
    public void visit(ProgramAST program) { }
    public void visit(VarDeclAST varDecl) { }
    public void visit(MultiVarDeclAST multiVarDecl) { }
    public void visit(ArrayAST array) { }
    public void visit(MultiDimArrayAST multiDimArray) { }
    public void visit(FuncDeclAST function) { }
    public void visit(ParameterAST parameter) { }
    public void visit(BlockAST block) { }
    public void visit(AssignAST assign) { }
    public void visit(MultiAssignAST multiAssign) { }
    public void visit(MultiAssignCallAST multiAssignCall) { }
    public void visit(ArrayAssignAST arrayAssign) { }
    public void visit(MultiDimArrayAssignAST multiDimArrayAssign) { }
    public void visit(ConditionalAST conditional) { }
    public void visit(WhileLoopAST whileLoop) { }
    public void visit(ReturnAST returnStmt) { }
    public void visit(BinaryExprAST binaryExpr) { }
    public void visit(UnaryExprAST unaryExpr) { }
    public void visit(VarAccessAST varAccess) { }
    public void visit(ArrayAccessAST arrayAccess) { }
    public void visit(MultiDimArrayAccessAST multiDimArrayAccess) { }
    public void visit(ProcedureCallAST procedureCall) { }
    public void visit(IntLiteralAST intLiteral) { }
    public void visit(BoolLiteralAST boolLiteral) { }
    public void visit(CharLiteralAST charLiteral) { }
    public void visit(StrLiteralAST strLiteral) { }
}