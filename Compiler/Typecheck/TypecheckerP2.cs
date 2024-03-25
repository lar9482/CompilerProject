using CompilerProj.Context;
using CompilerProj.Visitors;

/*
 * This pass does the actual full typechecking, via recursive visits and ASt annotation.
 */
public sealed class TypecheckerP2 : ASTVisitor {
    private Context context;
    private List<string> errorMsgs;

    public TypecheckerP2() {
        this.context = new Context();
        this.errorMsgs = new List<string>();
    }

    // Top level nodes
    public void visit(ProgramAST program) { 
        if (program.scope == null) {
            errorMsgs.Add("Semantic Error: Top level scope was not initialized");
            return;
        }

        context.push(program.scope);

        foreach(DeclAST decl in program.declarations) {
            decl.accept(this);
        }

        foreach(FunctionAST function in program.functions) {
            function.accept(this);
        }
        program.scope = context.pop();
    }

    public void visit(VarDeclAST varDecl) { }
    public void visit(MultiVarDeclAST multiVarDecl) { }
    public void visit(MultiVarDeclCallAST multiVarDeclCall) { }
    public void visit(ArrayDeclAST array) { }
    public void visit(MultiDimArrayDeclAST multiDimArray) { }
    public void visit(FunctionAST function) { }
    public void visit(ParameterAST parameter) { }
    public void visit(BlockAST block) { }

    // Stmt nodes
    public void visit(AssignAST assign) { }
    public void visit(MultiAssignAST multiAssign) { }
    public void visit(MultiAssignCallAST multiAssignCall) { }
    public void visit(ArrayAssignAST arrayAssign) { }
    public void visit(MultiDimArrayAssignAST multiDimArrayAssign) { }
    public void visit(ConditionalAST conditional) { }
    public void visit(WhileLoopAST whileLoop) { }
    public void visit(ReturnAST returnStmt) { }
    public void visit(ProcedureCallAST procedureCall) { }
    
    
    // Expr Nodes
    public void visit(BinaryExprAST binaryExpr) { }
    public void visit(UnaryExprAST unaryExpr) { }
    public void visit(VarAccessAST varAccess) { }
    public void visit(ArrayAccessAST arrayAccess) { }
    public void visit(MultiDimArrayAccessAST multiDimArrayAccess) { }
    public void visit(FunctionCallAST functionCall) { }
    public void visit(IntLiteralAST intLiteral) { }
    public void visit(BoolLiteralAST boolLiteral) { }
    public void visit(CharLiteralAST charLiteral) { }
    public void visit(StrLiteralAST strLiteral) { }
}