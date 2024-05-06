using CompilerProj.Visitors;

public sealed class ASTVisitorImplemented : ASTVisitorVoid {
    public void visit(ProgramAST program) { }
    public void visit(VarDeclAST varDecl) { }
    public void visit(MultiVarDeclAST multiVarDecl) { }
    public void visit(MultiVarDeclCallAST multiVarDeclCallAST) { }
    public void visit(ArrayDeclAST array) { }
    public void visit(MultiDimArrayDeclAST multiDimArray) { }
    public void visit(ArrayDeclCallAST arrayCall) { }
    public void visit(MultiDimArrayDeclCallAST multiDimArrayCall) { }

    public void visit(FunctionAST function) { }
    public void visit(ParameterAST parameter) { }
    public void visit(BlockAST block) { }
    public void visit(VarAssignAST varAssign) { }
    public void visit(VarMutateAST varMutate) { }
    public void visit(MultiAssignAST multiAssign) { }
    public void visit(MultiAssignCallAST multiAssignCall) { }
    public void visit(ArrayAssignAST arrayAssign) { }
    public void visit(ArrayMutateAST arrayMutate) { }
    public void visit(MultiDimArrayAssignAST multiDimArrayAssign) { }
    public void visit(ConditionalAST conditional) { }
    public void visit(WhileLoopAST whileLoop) { }
    public void visit(ForLoopAST forLoop) { }
    public void visit(ReturnAST returnStmt) { }
    public void visit(ProcedureCallAST procedureCall) { }
    
    public void visit(BinaryExprAST binaryExpr) { }
    public void visit(UnaryExprAST unaryExpr) { }
    public void visit(VarAccessAST varAccess) { }
    public void visit(ArrayAccessAST arrayAccess) { }
    public void visit(MultiDimArrayAccessAST multiDimArrayAccess) { }
    public void visit(MultiDimArrayMutateAST multiDimArrayMutate) { }
    public void visit(FunctionCallAST functionCall) { }
    public void visit(IntLiteralAST intLiteral) { }
    public void visit(BoolLiteralAST boolLiteral) { }
    public void visit(CharLiteralAST charLiteral) { }
    public void visit(StrLiteralAST strLiteral) { }
}