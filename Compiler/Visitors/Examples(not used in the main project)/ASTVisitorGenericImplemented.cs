namespace CompilerProj.Visitors;

public class DummyClass {

}

public class ASTVisitorGenericImplemented : ASTVisitorGeneric {

    private ExpectedType matchingType<ExpectedType, ActualType>(ActualType type) {
        if (type is ExpectedType specifiedType) {
            return specifiedType;
        } else {
            throw new Exception();
        }
    }
    
    // Top level nodes
    public T visit<T>(ProgramAST program) { 
        DummyClass test = new DummyClass();

        return matchingType<T, DummyClass>(test);
    }
    public T visit<T>(VarDeclAST varDecl) { throw new NotImplementedException(); }
    public T visit<T>(MultiVarDeclAST multiVarDecl) { throw new NotImplementedException(); }
    public T visit<T>(MultiVarDeclCallAST multiVarDeclCall) { throw new NotImplementedException(); }
    public T visit<T>(ArrayDeclAST array) { throw new NotImplementedException(); }
    public T visit<T>(MultiDimArrayDeclAST multiDimArray) { throw new NotImplementedException(); }
    public T visit<T>(ArrayDeclCallAST arrayCall) { throw new NotImplementedException(); }
    public T visit<T>(MultiDimArrayDeclCallAST multiDimArrayCall) { throw new NotImplementedException(); }
    public T visit<T>(FunctionAST function) { throw new NotImplementedException(); }
    public T visit<T>(ParameterAST parameter) { throw new NotImplementedException(); }
    public T visit<T>(BlockAST block) { throw new NotImplementedException(); }

    // Stmt nodes
    public T visit<T>(AssignAST assign) { throw new NotImplementedException(); }
    public T visit<T>(MultiAssignAST multiAssign) { throw new NotImplementedException(); }
    public T visit<T>(MultiAssignCallAST multiAssignCall) { throw new NotImplementedException(); }
    public T visit<T>(ArrayAssignAST arrayAssign) { throw new NotImplementedException(); }
    public T visit<T>(MultiDimArrayAssignAST multiDimArrayAssign) { throw new NotImplementedException(); }
    public T visit<T>(ConditionalAST conditional) { throw new NotImplementedException(); }
    public T visit<T>(WhileLoopAST whileLoop) { throw new NotImplementedException(); }
    public T visit<T>(ReturnAST returnStmt) { throw new NotImplementedException(); }
    public T visit<T>(ProcedureCallAST procedureCall) { throw new NotImplementedException(); }
    
    
    // Expr Nodes
    public T visit<T>(BinaryExprAST binaryExpr) { throw new NotImplementedException(); }
    public T visit<T>(UnaryExprAST unaryExpr) { throw new NotImplementedException(); }
    public T visit<T>(VarAccessAST varAccess) { throw new NotImplementedException(); }
    public T visit<T>(ArrayAccessAST arrayAccess) { throw new NotImplementedException(); }
    public T visit<T>(MultiDimArrayAccessAST multiDimArrayAccess) { throw new NotImplementedException(); }
    public T visit<T>(FunctionCallAST functionCall) { throw new NotImplementedException(); }
    public T visit<T>(IntLiteralAST intLiteral) { throw new NotImplementedException(); }
    public T visit<T>(BoolLiteralAST boolLiteral) { throw new NotImplementedException(); }
    public T visit<T>(CharLiteralAST charLiteral) { throw new NotImplementedException(); }
    public T visit<T>(StrLiteralAST strLiteral) { throw new NotImplementedException(); }
}