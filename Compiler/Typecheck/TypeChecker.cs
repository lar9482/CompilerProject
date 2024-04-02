using CompilerProj.Context;
using CompilerProj.Types;
using CompilerProj.Visitors;

/*
 * Wrapper for typechecking passes.
 */
public abstract class TypeChecker : ASTVisitor {
    protected Context context;
    protected List<string> errorMsgs;

    public TypeChecker() {
        this.context = new Context();
        this.errorMsgs = new List<string>();
    }

    public abstract void visit(ProgramAST program);
    public abstract void visit(FunctionAST function);
    public abstract void visit(VarDeclAST varDecl);
    public abstract void visit(MultiVarDeclAST multiVarDecl);
    public abstract void visit(MultiVarDeclCallAST multiVarDeclCallAST);
    public abstract void visit(ArrayDeclAST array);
    public abstract void visit(MultiDimArrayDeclAST multiDimArray);

    protected void initializeVarDecl(VarDeclAST varDecl) {
        if (context.lookup(varDecl.name) != null) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: {2} exists already.", 
                    varDecl.lineNumber, varDecl.columnNumber, varDecl.name
                )
            );
            return;
        }

        SymbolVariable symbolVar = new SymbolVariable(
            varDecl.name, varDecl.declType
        );

        context.put(varDecl.name, symbolVar);
    }

    protected void checkVarDecl(VarDeclAST varDecl) {
        SymbolVariable varSymbol = (SymbolVariable) lookUpSymbolFromContext(
            varDecl.name, varDecl.lineNumber, varDecl.columnNumber
        );

        if (varDecl.initialValue == null) {
            return;
        }
        varDecl.initialValue.accept(this);
        SimpleType initialValueType = varDecl.initialValue.type;
        SimpleType varDeclType = varSymbol.type;

        if (!sameTypes(initialValueType, varDeclType)) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: The initial value type, {2}, doesn't match with the variable declaration.",
                    varDecl.initialValue.lineNumber,
                    varDecl.initialValue.columnNumber,
                    simpleTypeToString(initialValueType)
                )
            );
        }
    }

    protected void initializeMultiVarDecl(MultiVarDeclAST multiVarDecl) {
        foreach(KeyValuePair<string, PrimitiveType> nameAndType in multiVarDecl.declTypes) {
            if (context.lookup(nameAndType.Key) != null) {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError:{2} exists already.", 
                        multiVarDecl.lineNumber, multiVarDecl.columnNumber, nameAndType.Key
                    )
                );
                continue;
            }

            SymbolVariable symbolVar = new SymbolVariable(
                nameAndType.Key,
                nameAndType.Value
            );

            context.put(nameAndType.Key, symbolVar);
        }
    }

    protected void checkMultiVarDecl(MultiVarDeclAST multiVarDecl) {
        foreach(string varName in multiVarDecl.names) {
            SymbolVariable varSymbol = (SymbolVariable) lookUpSymbolFromContext(
                varName, multiVarDecl.lineNumber, multiVarDecl.columnNumber
            );

            ExprAST? initialValue = multiVarDecl.initialValues[varName];
            if (initialValue == null) {
                continue;
            }

            initialValue.accept(this);
            SimpleType initialValueType = initialValue.type;

            if (!sameTypes(initialValueType, varSymbol.type)) {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: The initial value type, {2}, doesn't match with the variable declaration.",
                        initialValue.lineNumber,
                        initialValue.columnNumber,
                        simpleTypeToString(initialValueType)
                    )
                );
            }
        }
    }

    protected void initializeMultiVarDeclCall(MultiVarDeclCallAST multiVarDeclCall) {
        foreach(KeyValuePair<string, PrimitiveType> nameAndType in multiVarDeclCall.declTypes) {
            if (context.lookup(nameAndType.Key) != null) {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: {2} exists already.", 
                        multiVarDeclCall.lineNumber, multiVarDeclCall.columnNumber, nameAndType.Key
                    )
                );
                continue;
            }

            SymbolVariable symbolVar = new SymbolVariable(
                nameAndType.Key,
                nameAndType.Value
            );

            context.put(nameAndType.Key, symbolVar);
        }
    }

    protected void checkMultiVarDeclCall(MultiVarDeclCallAST multiVarDeclCall) {
        SymbolFunction symbolFunction = (SymbolFunction) lookUpSymbolFromContext(
            multiVarDeclCall.functionCall.functionName,
            multiVarDeclCall.lineNumber, multiVarDeclCall.columnNumber
        );

        //Checking variable declarations against the function return types.
        for (int i = 0; i < multiVarDeclCall.names.Count; i++) {
            string varName = multiVarDeclCall.names[i];
            SymbolVariable symbolVar = (SymbolVariable) lookUpSymbolFromContext(
                varName, multiVarDeclCall.lineNumber, multiVarDeclCall.columnNumber
            );

            SimpleType variableType = symbolVar.type;
            SimpleType returnFuncType = symbolFunction.returnTypes[i];
            if (!sameTypes(variableType , returnFuncType)) {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: {2}'s type doesn't match with the return type {3}",
                        multiVarDeclCall.lineNumber,
                        multiVarDeclCall.columnNumber,
                        varName, simpleTypeToString(returnFuncType)
                    )
                );
            }
        }

        //Checking parameter types
        FunctionCallAST functionCall = multiVarDeclCall.functionCall;
        for (int i = 0; i < functionCall.args.Count; i++) {
            ExprAST param = functionCall.args[i];
            param.accept(this);

            SimpleType paramType = param.type;
            SimpleType expectedParamType = symbolFunction.parameterTypes[i];
            
            if (!sameTypes(paramType, expectedParamType)) {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: Param's type {2} doesn't match with the expected type {3}",
                        param.lineNumber, param.columnNumber,
                        simpleTypeToString(paramType), simpleTypeToString(expectedParamType)
                    )
                );
            }
        }
    }

    protected void initializeArrayDecl(ArrayDeclAST array) {
        if (context.lookup(array.name) != null) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: {2} exists already.", 
                    array.lineNumber, array.columnNumber, array.name
                )
            );
            return;
        }

        SymbolVariable symbolVar = new SymbolVariable(
            array.name,
            array.declType
        );

        context.put(array.name, symbolVar);
    }

    protected void checkArrayDecl(ArrayDeclAST array) {

        //Checking the size is an integer
        array.size.accept(this);
        SimpleType arraySizeType = array.size.type;

        if (arraySizeType.TypeTag != "int") {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: The size expression must be an int",
                    array.size.lineNumber, array.size.columnNumber
                )
            );
        }

        //Checking all the initial values are integers or bools.
        if (array.initialValues == null) {
            return;
        }

        SymbolVariable symbolVar = (SymbolVariable) lookUpSymbolFromContext(
            array.name, array.lineNumber, array.columnNumber
        );

        SimpleType expectedInitValType;
        switch(symbolVar.type) {
            case ArrayType<PrimitiveType> arrayDecl when arrayDecl.baseType.TypeTag=="int": 
                expectedInitValType = new IntType();
                break;
            case ArrayType<PrimitiveType> arrayDecl when arrayDecl.baseType.TypeTag=="bool":
                expectedInitValType = new BoolType();
                break;
            default:
                throw new Exception(
                    String.Format(
                        "{0}:{1} SemanticError: The array type could not be resolved incorrectly"
                    )
                );
        }

        foreach(ExprAST initialValue in array.initialValues) {
            initialValue.accept(this);
            SimpleType actualInitValType = initialValue.type;

            if (!sameTypes(expectedInitValType, actualInitValType)) {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: The initial value must be of type {2}",
                        initialValue.lineNumber, initialValue.columnNumber,
                        simpleTypeToString(expectedInitValType)
                    )
                );
            }
        }
    }

    protected void initializeMultiDimArrayDecl(MultiDimArrayDeclAST multiDimArray) {
        if (context.lookup(multiDimArray.name) != null) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: {2} exists already.", 
                    multiDimArray.lineNumber, multiDimArray.columnNumber, multiDimArray.name
                )
            );
            return;
        }

        SymbolVariable symbolVar = new SymbolVariable(
            multiDimArray.name,
            multiDimArray.declType
        );

        context.put(multiDimArray.name, symbolVar);
    }

    protected void checkMultiDimArrayDecl(MultiDimArrayDeclAST multiDimArray) {

        //Checking that the row and column expression sizes are integers.
        multiDimArray.rowSize.accept(this);
        multiDimArray.colSize.accept(this);

        SimpleType rowSizeType = multiDimArray.rowSize.type;
        SimpleType colSizeType = multiDimArray.colSize.type;

        if (rowSizeType.TypeTag != "int") {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: The row size expression must be an int",
                    multiDimArray.rowSize.lineNumber, multiDimArray.rowSize.columnNumber
                )
            );
        }
        if (colSizeType.TypeTag != "int") {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: The column size expression must be an int",
                    multiDimArray.colSize.lineNumber, multiDimArray.colSize.columnNumber
                )
            );
        }

        //Checking that the initial values are either integers are bools.
        if (multiDimArray.initialValues == null) {
            return;
        }

        SymbolVariable symbolVar = (SymbolVariable) lookUpSymbolFromContext(
            multiDimArray.name, multiDimArray.lineNumber, multiDimArray.columnNumber
        );

        SimpleType expectedInitValType;
        switch(symbolVar.type) {
            case MultiDimArrayType<PrimitiveType> arrayDecl when arrayDecl.baseType.TypeTag=="int": 
                expectedInitValType = new IntType();
                break;
            case MultiDimArrayType<PrimitiveType> arrayDecl when arrayDecl.baseType.TypeTag=="bool":
                expectedInitValType = new BoolType();
                break;
            default:
                throw new Exception(
                    String.Format(
                        "{0}:{1} SemanticError: The array type could not be resolved incorrectly"
                    )
                );
        }

        for (int i = 0; i < multiDimArray.initialValues.Length; i++) {
            for (int j = 0; j < multiDimArray.initialValues[i].Length; j++) {
                ExprAST initialValue = multiDimArray.initialValues[i][j];
                initialValue.accept(this);

                SimpleType actualInitValType = initialValue.type;
                if (!sameTypes(expectedInitValType, actualInitValType)) {
                    errorMsgs.Add(
                        String.Format(
                            "{0}:{1} SemanticError: The initial value must be of type {2}",
                            initialValue.lineNumber, initialValue.columnNumber,
                            simpleTypeToString(actualInitValType)
                        )
                    );
                }
            }
        }
    }

    protected void initializeFunction(FunctionAST function) {
        if (context.lookup(function.name) != null) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: {2} exists already.",
                    function.lineNumber,
                    function.columnNumber,
                    function.name
                )
            );
            return;
        }

        List<SimpleType> parameterTypes = new List<SimpleType>();
        foreach(ParameterAST param in function.parameters) {
            parameterTypes.Add(param.type);
        }

        SymbolFunction symbolFunc = new SymbolFunction(
            function.name,
            parameterTypes.ToArray<SimpleType>(),
            function.returnTypes.ToArray<SimpleType>()
        );
        context.put(function.name, symbolFunc);
    }

    public void visit(ParameterAST parameter) { 
        if (context.lookup(parameter.name) != null) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: {2} exists already.",
                    parameter.lineNumber,
                    parameter.columnNumber,
                    parameter.name
                )
            );
            return;
        }

        SymbolVariable paramSymbol = new SymbolVariable(
            parameter.name,
            parameter.type
        );
        context.put(parameter.name, paramSymbol);
    }

    public void visit(BlockAST block) { 
        if (block.statements.Count == 0) {
            block.type = new UnitType();
            return;
        }

        context.push();

        for(int i = 0; i < block.statements.Count; i++) {
            StmtAST stmt = block.statements[i];
            stmt.accept(this);

            if (i < (block.statements.Count-1) && stmt.type.TypeTag == "terminate") {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: The next statement can't be executed, because of the current statement terminates",
                        stmt.lineNumber, stmt.columnNumber
                    )
                );
            }
        }

        block.scope = context.pop();    
        block.type = block.statements.Last<StmtAST>().type;
    }

    public void visit(ConditionalAST conditional) { 
        StmtType conditionalType;
        if (conditional.elseIfConditionalBlocks == null) {
            if (conditional.elseBlock == null) {
                conditionalType = checkIfStmt(
                    conditional.ifCondition, conditional.ifBlock
                );
            } else {
                conditionalType = checkIfElseStmt(
                    conditional.ifCondition, conditional.ifBlock, conditional.elseBlock
                );
            }
        } else {
            if (conditional.elseBlock == null) {
                conditionalType = checkIf_ElseIf_Stmt(
                    conditional.ifCondition,
                    conditional.ifBlock,
                    conditional.elseIfConditionalBlocks
                );
            } else {
                conditionalType = checkIf_ElseIf_ElseStmt(
                    conditional.ifCondition,
                    conditional.ifBlock,
                    conditional.elseIfConditionalBlocks,
                    conditional.elseBlock
                );
            }
        }

        conditional.type = conditionalType;
    }

    private StmtType checkIfStmt(ExprAST ifCondition, BlockAST ifBlock) {
        ifCondition.accept(this);

        if (ifCondition.type.TypeTag != "bool") {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: Condition for the if statement must be a bool",
                    ifCondition.lineNumber, ifCondition.columnNumber
                )
            );
        }
        
        ifBlock.accept(this);
        return new UnitType();
    }

    private StmtType checkIfElseStmt(ExprAST ifCondition, BlockAST ifBlock, BlockAST elseBlock) {
        ifCondition.accept(this);

        if (ifCondition.type.TypeTag != "bool") {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: Condition for the if statement must be a bool",
                    ifCondition.lineNumber, ifCondition.columnNumber
                )
            );
        }

        ifBlock.accept(this);
        elseBlock.accept(this);

        return (ifBlock.type.TypeTag == "terminate" && elseBlock.type.TypeTag == "terminate") ? (
            new TerminateType()
        ) : (
            new UnitType()
        );
    }

    private StmtType checkIf_ElseIf_Stmt(
        ExprAST ifCondition,
        BlockAST ifBlock,
        Dictionary<ExprAST, BlockAST> elseIfConditionalBlocks
    ) {
        ifCondition.accept(this);
        if (ifCondition.type.TypeTag != "bool") {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: Condition for the if statement must be a bool",
                    ifCondition.lineNumber, ifCondition.columnNumber
                )
            );
        }
        ifBlock.accept(this);
        bool unitTypeStmtExists = ifBlock.type.TypeTag == "unit";

        foreach(KeyValuePair<ExprAST, BlockAST> elseIfConditionalBlock in elseIfConditionalBlocks) {
            ExprAST elseIfCondition = elseIfConditionalBlock.Key;
            BlockAST elseIfBlock = elseIfConditionalBlock.Value;

            elseIfCondition.accept(this);
            if (elseIfCondition.type.TypeTag != "bool") {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: Condition for the else if statement must be a bool",
                        elseIfCondition.lineNumber, elseIfCondition.columnNumber
                    )
                );
            }
            elseIfBlock.accept(this);

            if (!unitTypeStmtExists && elseIfBlock.type.TypeTag == "unit") {
                unitTypeStmtExists = true;
            }
        }

        return (unitTypeStmtExists) ? (
            new UnitType()
        ) : (
            new TerminateType()
        );
    }

    private StmtType checkIf_ElseIf_ElseStmt(
        ExprAST ifCondition,
        BlockAST ifBlock,
        Dictionary<ExprAST, BlockAST> elseIfConditionalBlocks, 
        BlockAST elseBlock
    ) {
        StmtType ifElseIfType = checkIf_ElseIf_Stmt(ifCondition, ifBlock, elseIfConditionalBlocks);
        elseBlock.accept(this);

        return (ifElseIfType.TypeTag == "unit" || elseBlock.type.TypeTag == "unit") ? (
            new UnitType()
        ) : (
            new TerminateType()
        );
    }

    public void visit(WhileLoopAST whileLoop) { 
        whileLoop.condition.accept(this);
        SimpleType conditionType = whileLoop.condition.type;
        if (conditionType.TypeTag != "bool") {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: The while loop condition must be a bool",
                    whileLoop.condition.lineNumber,
                    whileLoop.condition.columnNumber
                )
            );
        }

        whileLoop.body.accept(this);
        whileLoop.type = new UnitType();
    }

    public void visit(AssignAST assign) { 
        assign.variable.accept(this);
        assign.value.accept(this);
        SimpleType expectedType = assign.variable.type;
        SimpleType actualType = assign.value.type;

        if (expectedType.TypeTag != actualType.TypeTag) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: The assignment expression type, {2}, doesn't match with the {3}'s type {4}",
                    assign.lineNumber,
                    assign.columnNumber,
                    simpleTypeToString(actualType),
                    assign.variable.variableName,
                    simpleTypeToString(expectedType)
                )
            );
        }

        assign.type = new UnitType();
    }

    public void visit(MultiAssignAST multiAssign) { }
    public void visit(MultiAssignCallAST multiAssignCall) { }
    public void visit(ArrayAssignAST arrayAssign) { }
    public void visit(MultiDimArrayAssignAST multiDimArrayAssign) { }
    public void visit(ReturnAST returnStmt) { }

    public void visit(ProcedureCallAST procedureCall) { 
        SymbolFunction symbolFunction = (SymbolFunction) lookUpSymbolFromContext(
            procedureCall.procedureName, procedureCall.lineNumber, procedureCall.columnNumber
        );

        if (symbolFunction.returnTypes.Length != 0) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: {2} is a function that returns a type, not a procedure",
                    procedureCall.lineNumber, procedureCall.columnNumber,
                    procedureCall.procedureName
                )
            );
        }

        if (symbolFunction.parameterTypes.Length != procedureCall.args.Count) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: The number of parameters for {2} must match the number of arguments.",
                    procedureCall.lineNumber, procedureCall.columnNumber,
                    procedureCall.procedureName
                )
            );
            procedureCall.type = new UnitType();
            return;
        } 

        for(int i = 0; i < symbolFunction.parameterTypes.Length; i++) {
            procedureCall.args[i].accept(this);

            SimpleType expectedType = symbolFunction.parameterTypes[i];
            SimpleType actualType = procedureCall.args[i].type;

            if (simpleTypeToString(expectedType) != simpleTypeToString(actualType)) {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: The type of the argument, {2}, doesn't match the parameter, {3}.",
                        procedureCall.args[i].lineNumber, procedureCall.args[i].columnNumber,
                        simpleTypeToString(actualType), simpleTypeToString(expectedType)
                    )
                );
            }
        }

        procedureCall.type = new UnitType();
    }

    public void visit(BinaryExprAST binaryExpr) { 
        binaryExpr.leftOperand.accept(this);
        binaryExpr.rightOperand.accept(this);

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

        SimpleType accessValueType = arrayAccess.accessValue.type;
        if (accessValueType.TypeTag != "int") {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: Access type to {2} must be an Int",
                    arrayAccess.accessValue.lineNumber, 
                    arrayAccess.accessValue.columnNumber, 
                    arrayAccess.arrayName
                )
            );
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

        SimpleType firstIndexType = multiDimArrayAccess.firstIndex.type;
        SimpleType secondIndexType = multiDimArrayAccess.secondIndex.type;

        if (firstIndexType.TypeTag != "int") {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: Row access type to {2} must be an int.",
                    multiDimArrayAccess.firstIndex.lineNumber, 
                    multiDimArrayAccess.firstIndex.columnNumber, 
                    multiDimArrayAccess.arrayName   
                )
            );
        }
        if (secondIndexType.TypeTag != "int") {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: Column access type to {2} must be an int.",
                    multiDimArrayAccess.secondIndex.lineNumber,
                    multiDimArrayAccess.secondIndex.columnNumber, 
                    multiDimArrayAccess.arrayName
                )
            );
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

    protected Symbol lookUpSymbolFromContext(string identifier, int lineNum, int colNum) {
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

    protected bool sameTypes(SimpleType type1, SimpleType type2) {
        return simpleTypeToString(type1) == simpleTypeToString(type2);
    }

    protected string simpleTypeToString(SimpleType type) {
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