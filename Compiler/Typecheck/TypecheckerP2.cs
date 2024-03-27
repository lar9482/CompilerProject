using CompilerProj.Context;
using CompilerProj.Visitors;
using CompilerProj.Types;

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

    public void visit(VarDeclAST varDecl) { 
        SymbolVariable symbol = (SymbolVariable) lookUpSymbolFromContext(
            varDecl.name, varDecl.lineNumber, varDecl.columnNumber
        );

        if (varDecl.initialValue == null) {
            return;
        }
    }

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
    public void visit(BinaryExprAST binaryExpr) { 
        binaryExpr.leftOperand.accept(this);
        binaryExpr.rightOperand.accept(this);

        if (binaryExpr.leftOperand.type == null) {
            throw new Exception(String.Format(
                "{0}:{1} SemanticError: Unable to resolve left operand type",
                binaryExpr.lineNumber, binaryExpr.columnNumber
            ));
        }
        if (binaryExpr.rightOperand.type == null) {
            throw new Exception(String.Format(
                "{0}:{1} SemanticError: Unable to resolve right operand type",
                binaryExpr.lineNumber, binaryExpr.columnNumber
            ));
        }

        SimpleType leftType = binaryExpr.leftOperand.type;
        SimpleType rightType = binaryExpr.rightOperand.type;
        
        if (leftType.TypeTag == "int" && rightType.TypeTag == "int") {
            checkBinaryExpr_Int(binaryExpr);
        } else if (leftType.TypeTag == "bool" && rightType.TypeTag == "bool") {
            checkBinaryExpr_Bool(binaryExpr);
        } else if (leftType.TypeTag == "array" && rightType.TypeTag == "array") {
            checkBinaryExpr_Array(binaryExpr);
        } else if (leftType.TypeTag == "multiDimArray" && rightType.TypeTag == "multiDimArray") {
            checkBinaryExpr_MultiDimArray(binaryExpr);
        } else {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: The left operand type, {3}, mismatches with the right operand type, {4}",
                    binaryExpr.lineNumber, 
                    binaryExpr.columnNumber, 
                    leftType.TypeTag,
                    rightType.TypeTag
                )
            );
        }
    }

    private void checkBinaryExpr_Int(BinaryExprAST binaryExpr) {
        switch(binaryExpr.exprType) {
            case BinaryExprType.ADD:
            case BinaryExprType.SUB:
            case BinaryExprType.MULT:
            case BinaryExprType.DIV:
            case BinaryExprType.MOD:
                binaryExpr.type = new IntType();
                break;
            case BinaryExprType.EQUAL:
            case BinaryExprType.NOTEQ:
            case BinaryExprType.LT:
            case BinaryExprType.LEQ:
            case BinaryExprType.GT:
            case BinaryExprType.GEQ:
                binaryExpr.type = new BoolType();
                break;
            default:
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: {3} can't operate on two ints.",
                        binaryExpr.lineNumber, 
                        binaryExpr.columnNumber, 
                        binaryExpr.exprType
                    )
                );
                break;
        }
    }

    private void checkBinaryExpr_Bool(BinaryExprAST binaryExpr) {
        switch(binaryExpr.exprType) {
            case BinaryExprType.EQUAL:
            case BinaryExprType.NOTEQ:
            case BinaryExprType.AND:
            case BinaryExprType.OR:
                binaryExpr.type = new BoolType();
                break;
            default:
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: {3} can't operate on two bools.",
                        binaryExpr.lineNumber, 
                        binaryExpr.columnNumber, 
                        binaryExpr.exprType
                    )
                );
                break;
        }
    }

    private void checkBinaryExpr_Array(BinaryExprAST binaryExpr) {
        switch(binaryExpr.exprType) {
            case BinaryExprType.EQUAL:
            case BinaryExprType.NOTEQ:
                binaryExpr.type = new BoolType();
                break;
            default:
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: {3} can't operate on two arrays.",
                        binaryExpr.lineNumber, 
                        binaryExpr.columnNumber, 
                        binaryExpr.exprType
                    )
                );
                break;
        }
    }

    private void checkBinaryExpr_MultiDimArray(BinaryExprAST binaryExpr) {
        switch(binaryExpr.exprType) {
            case BinaryExprType.EQUAL:
            case BinaryExprType.NOTEQ:
                binaryExpr.type = new BoolType();
                break;
            default:
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: {3} can't operate on two multi-dimensional arrays.",
                        binaryExpr.lineNumber, 
                        binaryExpr.columnNumber, 
                        binaryExpr.exprType
                    )
                );
                break;
        }
    }

    public void visit(UnaryExprAST unaryExpr) { 
        unaryExpr.operand.accept(this);
        if (unaryExpr.operand.type == null) {
            throw new Exception(String.Format(
                "{0}:{1} SemanticError: Unable to resolve operand type.",
                unaryExpr.lineNumber, unaryExpr.columnNumber
            ));
        }

        SimpleType operandType = unaryExpr.operand.type;
        switch(operandType) {
            case IntType intType:
                if (unaryExpr.exprType == UnaryExprType.NEGATE) {
                    unaryExpr.type = new IntType();
                } else {
                    errorMsgs.Add(
                        String.Format(
                            "{0}:{1} SemanticError: Negation can only operate on an int.",
                            unaryExpr.lineNumber, 
                            unaryExpr.columnNumber, 
                            unaryExpr.exprType
                        )
                    );
                }
                break;
            case BoolType boolType:
                if (unaryExpr.exprType == UnaryExprType.NOT) {
                    unaryExpr.type = new BoolType();
                } else {
                    errorMsgs.Add(
                        String.Format(
                            "{0}:{1} SemanticError: Not can only operate on an bool.",
                            unaryExpr.lineNumber, 
                            unaryExpr.columnNumber, 
                            unaryExpr.exprType
                        )
                    );
                }
                break;
            default:
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: The operand must be int or bool, not {3}.",
                        unaryExpr.lineNumber, 
                        unaryExpr.columnNumber, 
                        operandType.TypeTag
                    )
                );
                break;
        }
    }

    public void visit(VarAccessAST varAccess) { 
        SymbolVariable symbol = (SymbolVariable) lookUpSymbolFromContext(
            varAccess.variableName, varAccess.lineNumber, varAccess.columnNumber
        );

        SimpleType symbolType = symbol.type;
        varAccess.type = symbolType;
    }

    public void visit(ArrayAccessAST arrayAccess) { 
        arrayAccess.accessValue.accept(this);

        SymbolVariable symbol = (SymbolVariable) lookUpSymbolFromContext(
            arrayAccess.arrayName, arrayAccess.lineNumber, arrayAccess.columnNumber
        );

        if (arrayAccess.accessValue.type != null) {
            if (arrayAccess.accessValue.type.TypeTag != "int") {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: Access type to {2} must be an Int",
                        arrayAccess.accessValue.lineNumber, 
                        arrayAccess.accessValue.columnNumber, 
                        arrayAccess.arrayName
                    )
                );
            }
        }

        switch(symbol.type) {
            case ArrayType<PrimitiveType> array when array.baseType.TypeTag=="int": 
                arrayAccess.type = new IntType();
                break;
            case ArrayType<PrimitiveType> array when array.baseType.TypeTag=="bool":
                arrayAccess.type = new BoolType();
                break;
        }
    }

    public void visit(MultiDimArrayAccessAST multiDimArrayAccess) { 
        multiDimArrayAccess.firstIndex.accept(this);
        multiDimArrayAccess.secondIndex.accept(this);
        SymbolVariable symbol = (SymbolVariable) lookUpSymbolFromContext(
            multiDimArrayAccess.arrayName, multiDimArrayAccess.lineNumber, multiDimArrayAccess.columnNumber
        );

        if (multiDimArrayAccess.firstIndex.type != null) {
            if (multiDimArrayAccess.firstIndex.type.TypeTag != "int") {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: Row access type to {2} must be an int.",
                        multiDimArrayAccess.firstIndex.lineNumber, 
                        multiDimArrayAccess.firstIndex.columnNumber, 
                        multiDimArrayAccess.arrayName
                    )
                );
                return;
            }
        }
        if (multiDimArrayAccess.secondIndex.type != null) {
            if (multiDimArrayAccess.secondIndex.type.TypeTag != "int") {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: Column access type to {2} must be an int.",
                        multiDimArrayAccess.secondIndex.lineNumber,
                        multiDimArrayAccess.secondIndex.columnNumber, 
                        multiDimArrayAccess.arrayName
                    )
                );
                return;
            }
        }

        switch(symbol.type) {
            case MultiDimArrayType<PrimitiveType> multiArray when multiArray.baseType.TypeTag=="int": 
                multiDimArrayAccess.type = new IntType();
                break;
            case MultiDimArrayType<PrimitiveType> multiArray when multiArray.baseType.TypeTag=="bool": 
                multiDimArrayAccess.type = new BoolType();
                break;
        }
    }

    public void visit(FunctionCallAST functionCall) { 
        SymbolFunction symbol = (SymbolFunction) lookUpSymbolFromContext(
            functionCall.functionName, functionCall.lineNumber, functionCall.columnNumber
        );

        for (int i = 0; i < symbol.parameterTypes.Length; i++) {
            SimpleType expectedParamType = symbol.parameterTypes[i];

            ExprAST param = functionCall.args[i];
            param.accept(this);

            if (param.type != null) {
                SimpleType actualParamType = param.type;
                if (!sameTypes(expectedParamType, actualParamType)) {
                    errorMsgs.Add(
                        String.Format(
                            "{0}:{1} SemanticError: Param type is expected to be {2}, but it is {3}",
                            param.lineNumber, param.columnNumber, 
                            expectedParamType.TypeTag, actualParamType.TypeTag
                        )
                    );
                }
            }
        }

        if (symbol.returnTypes.Length != 1) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: The function call {2} is only allowed a single return type",
                    functionCall.lineNumber, functionCall.columnNumber, 
                    functionCall.functionName
                )
            );
        }

        functionCall.type = symbol.returnTypes[0];
    }

    public void visit(IntLiteralAST intLiteral) { 
        intLiteral.type = new IntType();
    }
    public void visit(BoolLiteralAST boolLiteral) { 
        boolLiteral.type = new BoolType();
    }
    public void visit(CharLiteralAST charLiteral) { 
        charLiteral.type = new IntType();
    }
    public void visit(StrLiteralAST strLiteral) { 
        strLiteral.type = new ArrayType<PrimitiveType>(new IntType());
    }

    private Symbol lookUpSymbolFromContext(string identifier, int lineNum, int colNum) {
        Symbol? symbol = context.lookup(identifier);
        if (symbol == null) {
            throw new Exception(
                String.Format(
                    "{0}:{1} SemanticError: {2} wasn't declared. Unable to resolve its type",
                    lineNum, colNum, identifier
                )
            );
        }

        return symbol;
    }

    private bool sameTypes(SimpleType type1, SimpleType type2) {
        return simpleTypeToString(type1) == simpleTypeToString(type2);
    }

    private string simpleTypeToString(SimpleType type) {
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
}