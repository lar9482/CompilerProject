using CompilerProj.Visitors;

/*
 * Making post order traversals from the expression AST to see if they were parsed correctly.
 * This will convert expressions into reverse polish notation to verify
 */
internal class ExprVisitor : ASTVisitor {
    public Queue<string> traverseRecord;

    public ExprVisitor() {
        this.traverseRecord = new Queue<string>();
    }

    public void visit(ExprAST exprAST) {
        switch(exprAST.GetType().ToString()) {
            case "BinaryExprAST": visit((BinaryExprAST) exprAST); break;
            case "UnaryExprAST": visit((UnaryExprAST) exprAST); break;
            case "VarAccessAST": visit((VarAccessAST) exprAST); break;
            case "ArrayAccessAST": visit((ArrayAccessAST) exprAST); break;
            case "MultiDimArrayAccessAST": visit((MultiDimArrayAccessAST) exprAST); break;
            case "ProcedureCallAST": visit((ProcedureCallAST) exprAST); break;
            case "IntLiteralAST": visit((IntLiteralAST) exprAST); break;
            case "BoolLiteralAST": visit((BoolLiteralAST) exprAST); break;
        }
    }

    public void visit(BinaryExprAST binaryExpr) { 
        binaryExpr.leftOperand.accept(this);
        binaryExpr.rightOperand.accept(this);
        traverseRecord.Enqueue(binaryOpToString(binaryExpr.exprType));
    }

    public void visit(UnaryExprAST unaryExpr) {
        unaryExpr.operand.accept(this); 
        traverseRecord.Enqueue(unaryOpToString(unaryExpr.exprType));
    }

    public void visit(VarAccessAST varAccess) { 
        traverseRecord.Enqueue(varAccess.variableName);
    }

    public void visit(ArrayAccessAST arrayAccess) { 
        traverseRecord.Enqueue(arrayAccess.arrayName);
        arrayAccess.accessValue.accept(this);
    }

    public void visit(MultiDimArrayAccessAST multiDimArrayAccess) { 
        traverseRecord.Enqueue(multiDimArrayAccess.arrayName);
        multiDimArrayAccess.firstIndex.accept(this);
        multiDimArrayAccess.secondIndex.accept(this);
    }

    public void visit(ProcedureCallAST procedureCall) { 
        traverseRecord.Enqueue(procedureCall.procedureName);

        foreach (ExprAST arg in procedureCall.args) {
            arg.accept(this);
        }
    }

    public void visit(IntLiteralAST intLiteral) { 
        traverseRecord.Enqueue(intLiteral.value.ToString());
    }

    public void visit(BoolLiteralAST boolLiteral) { 
        traverseRecord.Enqueue(boolLiteral.value.ToString());
    }

    public void visit(CharLiteralAST charLiteral) { }
    public void visit(StrLiteralAST strLiteral) { }

    private string binaryOpToString(BinaryExprType type) {
        switch(type) {
            case BinaryExprType.OR: return "||";
            case BinaryExprType.AND: return "&&";
            case BinaryExprType.EQUAL: return "==";
            case BinaryExprType.NOTEQ: return "!=";
            case BinaryExprType.LT: return "<";
            case BinaryExprType.LEQ: return "<=";
            case BinaryExprType.GEQ: return ">=";
            case BinaryExprType.GT: return ">";
            case BinaryExprType.ADD: return "+";
            case BinaryExprType.SUB: return "-";
            case BinaryExprType.MULT: return "*";
            case BinaryExprType.DIV: return "/";
            case BinaryExprType.MOD: return "%";
            default:
                throw new Exception(String.Format("{0} is not recognized as a valid binary operator", type.ToString()));
        }
    }

    private string unaryOpToString(UnaryExprType type) {
        switch(type) {
            case UnaryExprType.NOT: return "!";
            case UnaryExprType.NEGATE: return "-";
            default:
                throw new Exception(String.Format("{0} is not recognized as a valid unary operator", type.ToString()));
        }
    }
    
    //Unused in the this visitor.
    public void visit(ProgramAST program) { throw new NotImplementedException("This visit is not implemented here."); }
    public void visit(VarDeclAST varDecl) { throw new NotImplementedException("This visit is not implemented here."); }
    public void visit(MultiVarDeclAST multiVarDecl) { throw new NotImplementedException("This visit is not implemented here."); }
    public void visit(ArrayAST array) { throw new NotImplementedException("This visit is not implemented here."); }
    public void visit(MultiDimArrayAST multiDimArray) { throw new NotImplementedException("This visit is not implemented here."); }
    public void visit(FuncDeclAST function) { throw new NotImplementedException("This visit is not implemented here."); }
    public void visit(ParameterAST parameter) { throw new NotImplementedException("This visit is not implemented here."); }
    public void visit(BlockAST block) { throw new NotImplementedException("This visit is not implemented here."); }
    public void visit(AssignAST assign) { throw new NotImplementedException("This visit is not implemented here."); }
    public void visit(MultiAssignAST multiAssign) { throw new NotImplementedException("This visit is not implemented here."); }
    public void visit(MultiAssignCallAST multiAssignCall) { throw new NotImplementedException("This visit is not implemented here."); }
    public void visit(ArrayAssignAST arrayAssign) { throw new NotImplementedException("This visit is not implemented here."); }
    public void visit(MultiDimArrayAssignAST multiDimArrayAssign) { throw new NotImplementedException("This visit is not implemented here."); }
    public void visit(ConditionalAST conditional) { throw new NotImplementedException("This visit is not implemented here."); }
    public void visit(WhileLoopAST whileLoop) { throw new NotImplementedException("This visit is not implemented here."); }
    public void visit(ReturnAST returnStmt) { throw new NotImplementedException("This visit is not implemented here."); }
}