using CompilerProj.AST;

namespace CompilerProj.Visitors;

internal interface ASTVisitor {
    // Top level nodes
    void visit(ProgramAST program);
    void visit(VarDeclAST varDecl);
    void visit(MultiVarDeclAST multiVarDecl);
    void visit(ArrayAST array);
    void visit(MultiDimArrayAST multiDimArray);
    void visit(FuncDeclAST function);
    void visit(ParameterAST parameter);
    void visit(BlockAST block);

    // Stmt nodes
    void visit(AssignAST assign);
    void visit(MultiAssignAST multiAssign);
    void visit(MultiAssignCallAST multiAssignCall);
    void visit(ArrayAssignAST arrayAssign);
    void visit(MultiDimArrayAssignAST multiDimArrayAssign);
    void visit(ConditionalAST conditional);
    void visit(WhileLoopAST whileLoop);
    void visit(ReturnAST returnStmt);

    // Expr Nodes
    void visit(BinaryExprAST binaryExpr);
    void visit(UnaryExprAST unaryExpr);
    void visit(VarAccessAST varAccess);
    void visit(ArrayAccessAST arrayAccess);
    void visit(MultiDimArrayAccessAST multiDimArrayAccess);
    void visit(ProcedureCallAST procedureCall);
    void visit(IntLiteralAST intLiteral);
    void visit(BoolLiteralAST boolLiteral);
    void visit(CharLiteralAST charLiteral);
    void visit(StrLiteralAST strLiteral);
}