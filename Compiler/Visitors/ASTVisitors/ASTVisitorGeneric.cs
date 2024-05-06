namespace CompilerProj.Visitors;

public interface ASTVisitorGeneric {
    // Declarations
    public T visit<T>(ProgramAST program);
    public T visit<T>(VarDeclAST varDecl);
    public T visit<T>(MultiVarDeclAST multiVarDecl);
    public T visit<T>(MultiVarDeclCallAST multiVarDeclCall);
    public T visit<T>(ArrayDeclAST array);
    public T visit<T>(MultiDimArrayDeclAST multiDimArray);
    public T visit<T>(ArrayDeclCallAST arrayCall);
    public T visit<T>(MultiDimArrayDeclCallAST multiDimArrayCall);

    //Function support nodes.
    public T visit<T>(FunctionAST function);
    public T visit<T>(ParameterAST parameter);
    public T visit<T>(BlockAST block);

    // Stmt nodes
    public T visit<T>(VarAssignAST varAssign);
    public T visit<T>(VarMutateAST varMutate);
    public T visit<T>(MultiAssignAST multiAssign);
    public T visit<T>(MultiAssignCallAST multiAssignCall);
    public T visit<T>(ArrayAssignAST arrayAssign);
    public T visit<T>(ArrayMutateAST arrayMutate);
    public T visit<T>(MultiDimArrayAssignAST multiDimArrayAssign);
    public T visit<T>(MultiDimArrayMutateAST multiDimArrayMutate);
    public T visit<T>(ConditionalAST conditional);
    public T visit<T>(WhileLoopAST whileLoop);
    public T visit<T>(ForLoopAST forLoop);
    public T visit<T>(ReturnAST returnStmt);
    public T visit<T>(ProcedureCallAST procedureCall);
    
    
    // Expr Nodes
    public T visit<T>(BinaryExprAST binaryExpr);
    public T visit<T>(UnaryExprAST unaryExpr);
    public T visit<T>(VarAccessAST varAccess);
    public T visit<T>(ArrayAccessAST arrayAccess);
    public T visit<T>(MultiDimArrayAccessAST multiDimArrayAccess);
    public T visit<T>(FunctionCallAST functionCall);
    public T visit<T>(IntLiteralAST intLiteral);
    public T visit<T>(BoolLiteralAST boolLiteral);
    public T visit<T>(CharLiteralAST charLiteral);
    public T visit<T>(StrLiteralAST strLiteral);
}