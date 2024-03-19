using CompilerProj.Types;
using CompilerProj.Visitors;

/*
 * Traversing the AST, while ignoring the expressions. These are
 * tested separately in ExprVisitor.cs
 */
internal class TopLvlVisitor : ASTVisitor {
    public Queue<string> traversalRecord;

    public TopLvlVisitor() {
        this.traversalRecord = new Queue<string>();
    }

    public void visit(ProgramAST program) { 
        foreach (var varDecl in program.varDecls) {
            varDecl.accept(this);
        }

        foreach(var multiVarDecl in program.multiVarDecls) {
            multiVarDecl.accept(this);
        }

        foreach(var arrayDecl in program.arrayDecls) {
            arrayDecl.accept(this);
        }

        foreach(var multiDimArrayDecl in program.multiDimArrayDecls) {
            multiDimArrayDecl.accept(this);
        }

        foreach (var funcDecl in program.funcDecls) {
            funcDecl.accept(this);
        }
    }

    public void visit(VarDeclAST varDecl) { 
        traversalRecord.Enqueue(String.Format("{0}: {1}", varDecl.name, LangTypeToString(varDecl.type)));
        if (varDecl.initialValue != null) {
            traversalRecord.Enqueue("EXPR");
        }
    }

    public void visit(MultiVarDeclAST multiVarDecl) { 
        foreach (KeyValuePair<string, PrimitiveType> pair in multiVarDecl.types) {
            traversalRecord.Enqueue(
                String.Format("{0}: {1}", pair.Key, LangTypeToString(pair.Value))
            );

            ExprAST? initialValue;
            multiVarDecl.initialValues.TryGetValue(pair.Key, out initialValue);
            if (initialValue != null) {
                traversalRecord.Enqueue("EXPR");
            }
        }
    }

    public void visit(ArrayAST array) {  
        traversalRecord.Enqueue(
            String.Format("{0}: {1} SIZE:{2}", 
            array.name, LangTypeToString(array.type), array.size
        ));

        if (array.initialValues != null) {
            traversalRecord.Enqueue("ArrayEXPRS");
        }
    }

    public void visit(MultiDimArrayAST multiDimArray) { 
        traversalRecord.Enqueue(
            String.Format("{0}: {1} ROW:{2} COL:{3}", 
            multiDimArray.name, LangTypeToString(multiDimArray.type), 
            multiDimArray.rowSize, multiDimArray.colSize
        ));

        if (multiDimArray.initialValues != null) {
            traversalRecord.Enqueue("MultiDimEXPRS");
        }
    }

    public void visit(FuncDeclAST function) { 
        traversalRecord.Enqueue(function.name);

        foreach (var parameter in function.parameters) {
            parameter.accept(this);
        }

        foreach (var returnType in function.returnTypes) {
            traversalRecord.Enqueue(LangTypeToString(returnType));
        }

        function.block.accept(this);
    }

    public void visit(ParameterAST parameter) { 
        traversalRecord.Enqueue(String.Format("{0}: {1}", parameter.name, LangTypeToString(parameter.type)));
    }

    public void visit(BlockAST block) { 
        foreach (var varDecl in block.varDecls) {
            varDecl.accept(this);
        }
        foreach(var multiVarDecl in block.multiVarDecls) {
            multiVarDecl.accept(this);
        }
        foreach(var arrayDecl in block.arrays) {
            arrayDecl.accept(this);
        }
        foreach(var multiDimArrayDecl in block.multiDimArrays) {
            multiDimArrayDecl.accept(this);
        }

        foreach(StmtAST Stmt in block.statements) {
            switch(Stmt) {
                case AssignAST assign: assign.accept(this); break;
                case MultiAssignAST multiAssign: multiAssign.accept(this); break;
                case MultiAssignCallAST multiAssignCall: multiAssignCall.accept(this); break;
                case ArrayAssignAST arrayAssign: arrayAssign.accept(this); break;
                case MultiDimArrayAssignAST multiDimArrayAssign: multiDimArrayAssign.accept(this); break;
                case ConditionalAST conditional: conditional.accept(this); break;
                case WhileLoopAST whileLoop: whileLoop.accept(this); break;
                case ReturnAST returnStmt: returnStmt.accept(this); break;
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

    private string LangTypeToString(LangType type) {
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

    public void visit(ProcedureCallAST procedureCall) {
        traversalRecord.Enqueue(procedureCall.procedureName);

        foreach (var arg in procedureCall.args) {
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