using CompilerProj.Types;
using CompilerProj.Visitors;

/*
 * Traversing the AST, while ignoring the expressions. These are
 * tested separately in ExprVisitor.cs
 */
internal class TopLvlVisitor : ASTVisitorVoid {
    public Queue<string> traversalRecord;

    public TopLvlVisitor() {
        this.traversalRecord = new Queue<string>();
    }

    public void visit(ProgramAST program) { 
        foreach (DeclAST decl in program.declarations) {
            switch(decl) {
                case VarDeclAST varDecl: visit(varDecl); break;
                case MultiVarDeclAST multiVarDecl: visit(multiVarDecl); break;
                case ArrayDeclAST arrayDecl: visit(arrayDecl); break;
                case MultiDimArrayDeclAST multiDimArrayDecl: visit(multiDimArrayDecl); break;
            }
        }

        foreach (var funcDecl in program.functions) {
            funcDecl.accept(this);
        }
    }

    public void visit(VarDeclAST varDecl) { 
        traversalRecord.Enqueue(String.Format("{0}: {1}", varDecl.name, SimpleTypeToString(varDecl.declType)));
        if (varDecl.initialValue != null) {
            traversalRecord.Enqueue("EXPR");
        }
    }

    public void visit(MultiVarDeclAST multiVarDecl) { 
        foreach (KeyValuePair<string, PrimitiveType> pair in multiVarDecl.declTypes) {
            traversalRecord.Enqueue(
                String.Format("{0}: {1}", pair.Key, SimpleTypeToString(pair.Value))
            );

            ExprAST? initialValue = multiVarDecl.initialValues.GetValueOrDefault(pair.Key);
            if (initialValue != null) {
                traversalRecord.Enqueue("EXPR");
            }
        }
    }

    public void visit(MultiVarDeclCallAST multiVarDeclCall) {
        foreach (KeyValuePair<string, PrimitiveType> pair in multiVarDeclCall.declTypes) {
            traversalRecord.Enqueue(
                String.Format("{0}: {1}", pair.Key, SimpleTypeToString(pair.Value))
            );
        }
        multiVarDeclCall.functionCall.accept(this);
    }
    
    public void visit(ArrayDeclAST array) {  
        traversalRecord.Enqueue(
            String.Format("{0}: {1}", 
            array.name, SimpleTypeToString(array.declType)
        ));

        if (array.initialValues != null) {
            traversalRecord.Enqueue("ArrayEXPRS");
        }
    }

    public void visit(MultiDimArrayDeclAST multiDimArray) { 
        traversalRecord.Enqueue(
            String.Format("{0}: {1}", 
            multiDimArray.name, SimpleTypeToString(multiDimArray.declType)
        ));

        if (multiDimArray.initialValues != null) {
            traversalRecord.Enqueue("MultiDimEXPRS");
        }
    }

    public void visit(FunctionAST function) { 
        traversalRecord.Enqueue(function.name);

        foreach (var parameter in function.parameters) {
            parameter.accept(this);
        }

        foreach (var returnType in function.returnTypes) {
            traversalRecord.Enqueue(SimpleTypeToString(returnType));
        }

        function.block.accept(this);
    }

    public void visit(ParameterAST parameter) { 
        traversalRecord.Enqueue(String.Format("{0}: {1}", parameter.name, SimpleTypeToString(parameter.type)));
    }

    public void visit(BlockAST block) { 

        foreach(StmtAST Stmt in block.statements) {
            switch(Stmt) {
                case VarDeclAST varDecl: visit(varDecl); break;
                case MultiVarDeclAST multiVarDecl: visit(multiVarDecl); break;
                case ArrayDeclAST arrayDecl: visit(arrayDecl); break;
                case MultiDimArrayDeclAST multiDimArrayDecl: visit(multiDimArrayDecl); break;
                case AssignAST assign: assign.accept(this); break;
                case MultiAssignAST multiAssign: multiAssign.accept(this); break;
                case MultiAssignCallAST multiAssignCall: multiAssignCall.accept(this); break;
                case ArrayAssignAST arrayAssign: arrayAssign.accept(this); break;
                case MultiDimArrayAssignAST multiDimArrayAssign: multiDimArrayAssign.accept(this); break;
                case ConditionalAST conditional: conditional.accept(this); break;
                case WhileLoopAST whileLoop: whileLoop.accept(this); break;
                case ReturnAST returnStmt: returnStmt.accept(this); break;
                case ProcedureCallAST procedureCall: procedureCall.accept(this); break;
                default:
                    break;
            }
        }
    }

    public void visit(AssignAST assign) { 
        assign.variable.accept(this);
        traversalRecord.Enqueue("EXPR");
    }

    public void visit(MultiAssignAST multiAssign) { 
        foreach (KeyValuePair<VarAccessAST, ExprAST> pair in multiAssign.assignments) {
            pair.Key.accept(this);
            traversalRecord.Enqueue("EXPR");
        }
    }

    public void visit(MultiAssignCallAST multiAssignCall) { 
        foreach (var variableName in multiAssignCall.variableNames) {
            variableName.accept(this);
        }
        multiAssignCall.call.accept(this);
    }

    public void visit(ArrayAssignAST arrayAssign) { 
        arrayAssign.arrayAccess.accept(this);
        traversalRecord.Enqueue("EXPR");
    }

    public void visit(MultiDimArrayAssignAST multiDimArrayAssign) { 
        multiDimArrayAssign.arrayAccess.accept(this);
        traversalRecord.Enqueue("EXPR");
    }

    public void visit(ConditionalAST conditional) { 
        traversalRecord.Enqueue("if (EXPR)");
        conditional.ifBlock.accept(this);

        if (conditional.elseIfConditionalBlocks == null) return;
        
        foreach(KeyValuePair<ExprAST, BlockAST> block in conditional.elseIfConditionalBlocks) {
            traversalRecord.Enqueue("else if (EXPR)");
            block.Value.accept(this);
        }
        if (conditional.elseBlock == null) return;

        traversalRecord.Enqueue(String.Format("else"));
        conditional.elseBlock.accept(this);
    }
    public void visit(WhileLoopAST whileLoop) { 
        traversalRecord.Enqueue("while (EXPR)");
        whileLoop.body.accept(this);
    }

    public void visit(ReturnAST returnStmt) { 
        traversalRecord.Enqueue("return");
        if (returnStmt.returnValues == null)
            return;

        foreach (var returnValue in returnStmt.returnValues) {
            traversalRecord.Enqueue("EXPR");
        }
    }

    public void visit(ProcedureCallAST procedureCall) {
        traversalRecord.Enqueue(procedureCall.procedureName);

        foreach (var arg in procedureCall.args) {
            traversalRecord.Enqueue("EXPR");
        }
    }

    private string SimpleTypeToString(SimpleType type) {
        switch(type) {
            case IntType intType: return "int";
            case BoolType boolType: return "bool";
            case ArrayType<PrimitiveType> array when array.baseType.TypeTag=="int": 
                return "int[]";
            case ArrayType<PrimitiveType> array when array.baseType.TypeTag=="bool": 
                return "bool[]";
            case MultiDimArrayType<PrimitiveType> multiArray when multiArray.baseType.TypeTag=="int": 
                return "int[][]";
            case MultiDimArrayType<PrimitiveType> multiArray when multiArray.baseType.TypeTag=="bool": 
                return "bool[][]";
            default: 
                throw new Exception(String.Format("Can't recognize the type"));
        }
    }

    public void visit(VarAccessAST varAccess) { 
        traversalRecord.Enqueue(varAccess.variableName);
    }

    public void visit(ArrayAccessAST arrayAccess) { 
        traversalRecord.Enqueue(String.Format("{0}[EXPR]", arrayAccess.arrayName));
    }

    public void visit(MultiDimArrayAccessAST multiDimArrayAccess) { 
        traversalRecord.Enqueue(String.Format("{0}[EXPR][EXPR]", multiDimArrayAccess.arrayName));
    }

    public void visit(FunctionCallAST functionCall) {
        traversalRecord.Enqueue(functionCall.functionName);

        foreach (var arg in functionCall.args) {
            traversalRecord.Enqueue("EXPR");
        }
    }

    // Unused
    public void visit(BinaryExprAST binaryExpr) { }
    public void visit(UnaryExprAST unaryExpr) {}
    public void visit(IntLiteralAST intLiteral) {}
    public void visit(BoolLiteralAST boolLiteral) { }
    public void visit(CharLiteralAST charLiteral) { }
    public void visit(StrLiteralAST strLiteral) { }
}