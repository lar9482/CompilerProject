using System.Reflection.Emit;
using CompilerProj.Context;
using CompilerProj.Visitors;

public sealed class IRGenerator : ASTVisitorGeneric {
    private Context context;
    private int labelCounter;
    private int argCounter;

    public IRGenerator() {
        this.context = new Context();
        this.labelCounter = 0;
        this.argCounter = 1;
    }

    // Top level nodes
    public T visit<T>(ProgramAST program) { 
        if (program.scope == null) {
            throw new Exception(
                String.Format(
                    "IRGenerator: Top level scope was not initialized",
                    program.lineNumber, program.columnNumber
                )
            );
        } 

        context.push(program.scope);

        Dictionary<string, IRFuncDecl> irFuncDecls = new Dictionary<string, IRFuncDecl>();
        foreach(FunctionAST functionAST in program.functions) {
            IRFuncDecl irFuncDecl = functionAST.accept<IRFuncDecl>(this);
            irFuncDecls.Add(functionAST.name, irFuncDecl);
        }

        program.scope = context.pop();
        return matchThenReturn<T, IRCompUnit>(
            new IRCompUnit(
                "program", 
                irFuncDecls,
                new List<string>() { }
            )
        );
    }

    public T visit<T>(VarDeclAST varDecl) { 
        //TODO: Handle empty expressions.
        if (varDecl.initialValue == null) {
            throw new Exception("IRGenerator: Handle empty expressions later");
        }

        IRExpr srcExpr = varDecl.initialValue.accept<IRExpr>(this);
        IRMove irMove = new IRMove(
            new IRTemp(varDecl.name),
            srcExpr
        );

        return matchThenReturn<T, IRMove>(irMove);
    }

    public T visit<T>(MultiVarDeclAST multiVarDecl) { 
        List<IRStmt> irStmts = new List<IRStmt>();

        foreach(KeyValuePair<string, ExprAST?> varWithInitVal in multiVarDecl.initialValues) {
            string varDecl = varWithInitVal.Key;
            ExprAST? initialVal = varWithInitVal.Value;

            //TODO: Handle empty expressions.
            if (initialVal == null) {
                throw new Exception("IRGenerator: Handle empty expressions later");
            }

            IRExpr irInitial = initialVal.accept<IRExpr>(this);
            IRMove irMove = new IRMove(
                new IRTemp(varDecl),
                irInitial
            );
            irStmts.Add(irMove);
        }

        return matchThenReturn<T, IRSeq>(
            new IRSeq(irStmts)
        );
    }

    public T visit<T>(MultiVarDeclCallAST multiVarDeclCall) { 
        SymbolFunction symbolFunction = lookUpSymbolFromContext<SymbolFunction>(
            multiVarDeclCall.functionName, -1, -1
        );

        List<IRStmt> irStmts = new List<IRStmt>();
        List<IRExpr> irFuncArgs = new List<IRExpr>();
        
        foreach(ExprAST funcArgAST in multiVarDeclCall.args) {
            IRExpr irFuncArg = funcArgAST.accept<IRExpr>(this);
            irFuncArgs.Add(irFuncArg);
        }

        irStmts.Add(
            new IRCallStmt(
                new IRName(multiVarDeclCall.functionName),
                irFuncArgs,
                symbolFunction.returnTypes.Length
            )
        );

        for (int i = 0; i < multiVarDeclCall.names.Count; i++) {
            string variableName = multiVarDeclCall.names[i];
            IRExpr irSrc = new IRTemp(IRConfiguration.ABSTRACT_RET_PREFIX + (i+1));
            IRTemp irDest = new IRTemp(variableName);

            IRMove irAssign = new IRMove(irDest, irSrc);
            irStmts.Add(irAssign);
        }

        IRSeq irSequence = new IRSeq(irStmts);
        return matchThenReturn<T, IRSeq>(irSequence);
    }

    public T visit<T>(ArrayDeclAST array) {
        if (array.initialValues == null) {
            return matchThenReturn<T, IRSeq>(
                allocateArrayDecl_WithoutExpr(array.name, array.size)
            );
        } else {
            return matchThenReturn<T, IRSeq>(
                allocateArrayDecl_WithExpr(
                    array.name,
                    array.initialValues
                )
            );
        }
    }

    /**
     * Translating S(x: t[e])
     */
    private IRSeq allocateArrayDecl_WithoutExpr(string arrayName, ExprAST arraySize) {
        IRTemp tSize = new IRTemp(String.Format("{0}Size", arrayName));
        IRTemp tArray = new IRTemp(String.Format("{0}A", arrayName));

        // Step1: Computing size of the array, then moving it into "tSize"
        IRExpr irSize = arraySize.accept<IRExpr>(this);
        IRMove computeSize = new IRMove(
            tSize,
            irSize
        );

        //Step 2: Using "tSize", allocating memory and storing the start address in "tArray"
        IRBinOp ir_BytesToAlloc = new IRBinOp(
            BinOpType.ADD,
            new IRBinOp(
                BinOpType.MUL,
                tSize,
                new IRConst(IRConfiguration.wordSize)
            ),
            new IRConst(IRConfiguration.wordSize)
        );
        IRCall irCallMalloc = new IRCall(
            new IRName("malloc"),
            new List<IRExpr>() { ir_BytesToAlloc }
        );
        IRMove allocateMem = new IRMove(
            new IRTemp(String.Format("{0}A", arrayName)),
            irCallMalloc
        );

        //Step 3: Using "tArrayAddr", place the size at that address in memory.
        IRMove storeSizeInMem = new IRMove(
            new IRMem(MemType.NORMAL, tArray),
            new IRTemp(String.Format("{0}Size", arrayName))
        );

        //Step 4: A register named after the array will now hold the starting address for the array.
        IRMove createArrayRegister = new IRMove(
            new IRTemp(arrayName),
            new IRBinOp(
                BinOpType.ADD,
                tArray,
                new IRConst(IRConfiguration.wordSize)
            )
        );

        return new IRSeq(new List<IRStmt>() {
            computeSize,
            allocateMem,
            storeSizeInMem,
            createArrayRegister
        });
    }
    
    private IRSeq allocateArrayDecl_WithExpr(string arrayName, ExprAST[] initialValues) {
        IRTemp tM = new IRTemp(
            String.Format(
                "{0}A", arrayName
            )
        );

        IRBinOp bytesToAllocate = new IRBinOp(
            BinOpType.ADD,
            new IRBinOp(
                BinOpType.MUL,
                new IRConst(initialValues.Length),
                new IRConst(IRConfiguration.wordSize)
            ),
            new IRConst(IRConfiguration.wordSize)
        );

        IRCall callMalloc = new IRCall(
            new IRName("malloc"),
            new List<IRExpr>() { bytesToAllocate }
        );

        List<IRStmt> allMoves = new List<IRStmt>();
        IRMove moveMallocIntoTM = new IRMove(
            tM, callMalloc
        );
        IRMove move_Length_Into_DereferencedTM = new IRMove(
            new IRMem(MemType.NORMAL, tM),
            new IRConst(initialValues.Length)
        );
        allMoves.Add(moveMallocIntoTM);
        allMoves.Add(move_Length_Into_DereferencedTM);

        for (int i = 0; i < initialValues.Length; i++) {
            IRMem dereferencedOffsetFromTM = new IRMem(
                MemType.NORMAL, new IRBinOp(
                    BinOpType.ADD,
                    tM,
                    new IRConst((i+1)*IRConfiguration.wordSize)
                )
            );
            IRExpr initialValue = initialValues[i].accept<IRExpr>(this);

            IRMove move_InitVal_Into_DereferencedOffsetFromTM = new IRMove(
                dereferencedOffsetFromTM,
                initialValue
            );
            allMoves.Add(
                move_InitVal_Into_DereferencedOffsetFromTM
            );
        }
        IRMove createArrayRefReg = new IRMove(
            new IRTemp(arrayName),
            new IRBinOp(
                BinOpType.ADD,
                tM, new IRConst(IRConfiguration.wordSize)
            )
        );
        allMoves.Add(createArrayRefReg);

        return new IRSeq(
            allMoves
        );
    }

    //TODO: Implement these
    public T visit<T>(MultiDimArrayDeclAST multiDimArray) { 
        throw new NotImplementedException(); 
    }

    public T visit<T>(FunctionAST function) { 
        if (function.scope == null) {
            throw new Exception(
                String.Format(
                    "IRGenerator: Function scope was not initialized",
                    function.lineNumber, function.columnNumber
                )
            );
        } 

        context.push(function.scope);

        List<IRStmt> irParamsAssigns = new List<IRStmt>();
        foreach(ParameterAST parameterAST in function.parameters) {
            IRMove irParam = parameterAST.accept<IRMove>(this);
            irParamsAssigns.Add(irParam);
        }

        IRSeq irParams = new IRSeq(irParamsAssigns);
        IRSeq irBlock = function.block.accept<IRSeq>(this);
        List<IRStmt> funcInstructions = irParams.statements.Concat(irBlock.statements).ToList();

        //Edge case for procedures that don't "return" anything
        //An return instruction will be added to exit the function body.
        if (function.returnTypes.Count == 0) {
            funcInstructions.Add(
                new IRReturn(
                    new List<IRExpr>() {}
                )
            );
        } 

        IRSeq irFuncBody = new IRSeq(
            new List<IRStmt>() {
                new IRLabel(function.name),
                new IRSeq(funcInstructions)
            }
        );

        argCounter = 1;
        function.scope = context.pop();
        return matchThenReturn<T, IRFuncDecl>(
            new IRFuncDecl(function.name, irFuncBody)
        );
    }

    public T visit<T>(ParameterAST parameter) { 
        IRMove irAssign = new IRMove(
            new IRTemp(parameter.name),
            new IRTemp(
                IRConfiguration.ABSTRACT_ARG_PREFIX + argCounter
            )
        );

        argCounter++;
        return matchThenReturn<T, IRMove>(
            irAssign
        );
    }

    public T visit<T>(BlockAST block) { 
        if (block.scope == null) {
            throw new Exception(
                String.Format(
                    "IRGenerator: Block scope was not initialized",
                    block.lineNumber, block.columnNumber
                )
            );
        } 

        context.push(block.scope);

        List<IRStmt> irStmts = new List<IRStmt>();

        foreach(StmtAST stmtAST in block.statements) {
            IRStmt irStmt = stmtAST.accept<IRStmt>(this);
            irStmts.Add(irStmt);
        }

        block.scope = context.pop();
        return matchThenReturn<T, IRSeq>(
            new IRSeq(irStmts)
        );
    }

    public T visit<T>(AssignAST assign) { 
        IRExpr irSrc = assign.value.accept<IRExpr>(this);
        IRTemp irDest = assign.variable.accept<IRTemp>(this);

        IRMove irAssign = new IRMove(
            irDest, irSrc
        );

        return matchThenReturn<T, IRMove>(irAssign);
    }

    public T visit<T>(MultiAssignAST multiAssign) { 
        List<IRStmt> irAssigns = new List<IRStmt>();

        foreach(KeyValuePair<VarAccessAST, ExprAST> assignAST in multiAssign.assignments) {
            ExprAST srcAST = assignAST.Value;
            VarAccessAST destAST = assignAST.Key;

            IRExpr irSrc = srcAST.accept<IRExpr>(this);
            IRTemp irDest = destAST.accept<IRTemp>(this);

            IRMove irAssign = new IRMove(
                irDest, irSrc
            );
            irAssigns.Add(irAssign);
        }

        IRSeq irSeq = new IRSeq(irAssigns);

        return matchThenReturn<T, IRSeq>(irSeq);
    }

    public T visit<T>(MultiAssignCallAST multiAssignCall) { 
        SymbolFunction symbolFunction = lookUpSymbolFromContext<SymbolFunction>(
            multiAssignCall.functionName, -1, -1
        );

        List<IRStmt> irStmts = new List<IRStmt>();
        List<IRExpr> irFuncArgs = new List<IRExpr>();
        
        foreach(ExprAST funcArgAST in multiAssignCall.args) {
            IRExpr irFuncArg = funcArgAST.accept<IRExpr>(this);
            irFuncArgs.Add(irFuncArg);
        }

        irStmts.Add(
            new IRCallStmt(
                new IRName(multiAssignCall.functionName),
                irFuncArgs,
                symbolFunction.returnTypes.Length
            )
        );

        for (int i = 0; i < multiAssignCall.variableNames.Count; i++) {
            VarAccessAST variable = multiAssignCall.variableNames[i];
            IRExpr irSrc = new IRTemp(IRConfiguration.ABSTRACT_RET_PREFIX + (i+1));
            IRTemp irDest = variable.accept<IRTemp>(this);

            IRMove irAssign = new IRMove(irDest, irSrc);
            irStmts.Add(irAssign);
        }

        IRSeq irSequence = new IRSeq(irStmts);
        return matchThenReturn<T, IRSeq>(irSequence);
    }
    
    public T visit<T>(ArrayAssignAST arrayAssign) { 
        IR_Eseq computeThenDeref_accessAddr = arrayAssign.arrayAccess.accept<IR_Eseq>(this);
        if (computeThenDeref_accessAddr.stmt.GetType() != typeof(IRSeq)) {
            throw new Exception("Computation of the access access was expected to be an IRSeq");
        }

        if (computeThenDeref_accessAddr.expr.GetType() != typeof(IRMem)) {
            throw new Exception("Dereference of the access access was expected to be an IRMem");
        }

        IRSeq computeAccessAddr = (IRSeq) computeThenDeref_accessAddr.stmt;
        IRMem derefAccessAddr = (IRMem) computeThenDeref_accessAddr.expr;
        IRExpr assignValue = arrayAssign.value.accept<IRExpr>(this);

        // Mem[tA + 4*tI] <- assignValue
        IRMove moveValueIntoDeref = new IRMove(
            derefAccessAddr,
            assignValue
        );

        List<IRStmt> allStmts = computeAccessAddr.statements;
        allStmts.Add(moveValueIntoDeref);
        IRSeq compute_Deref_ThenAssign = new IRSeq(
            allStmts
        );

        return matchThenReturn<T, IRSeq>(compute_Deref_ThenAssign);
    }

    //TODO: Implement conditionals, while loops, and array assigns.
    public T visit<T>(MultiDimArrayAssignAST multiDimArrayAssign) { throw new NotImplementedException(); }
    public T visit<T>(ConditionalAST conditional) { throw new NotImplementedException(); }

    public T visit<T>(WhileLoopAST whileLoop) { 
        IRLabel beginLoopLabel = createNewLabel();
        IRLabel trueLabel = createNewLabel();
        IRLabel falseLabel = createNewLabel();

        IRStmt conditionIR = translateBoolExprByCF(
            whileLoop.condition, trueLabel, falseLabel
        );

        IRSeq bodyIR = whileLoop.body.accept<IRSeq>(this);

        return matchThenReturn<T, IRSeq>(new IRSeq(new List<IRStmt>() {
            beginLoopLabel,
            conditionIR,
            trueLabel,
            bodyIR,
            new IRJump(new IRName(beginLoopLabel.name)),
            falseLabel
        }));
    }

    public T visit<T>(ReturnAST returnStmt) { 
        List<IRExpr> irReturns = new List<IRExpr>();
        
        if (returnStmt.returnValues == null) {
            return matchThenReturn<T, IRReturn>(
                new IRReturn(irReturns)
            );
        }

        foreach(ExprAST returnExpr in returnStmt.returnValues) {
            IRExpr irReturn = returnExpr.accept<IRExpr>(this);
            irReturns.Add(irReturn);
        }

        return matchThenReturn<T, IRReturn>(
            new IRReturn(irReturns)
        );
    }

    public T visit<T>(ProcedureCallAST procedureCall) { 
        List<IRExpr> irArgs = new List<IRExpr>();

        foreach(ExprAST argAST in procedureCall.args) {
            IRExpr irArg = argAST.accept<IRExpr>(this);
            irArgs.Add(irArg);
        }

        IRCallStmt callStmt = new IRCallStmt(
            new IRName(procedureCall.procedureName),
            irArgs,
            0
        );

        return matchThenReturn<T, IRCallStmt>(callStmt);
    }
    
    public T visit<T>(BinaryExprAST binaryExpr) { 
        IRExpr irLeft = binaryExpr.leftOperand.accept<IRExpr>(this);
        IRExpr irRight = binaryExpr.rightOperand.accept<IRExpr>(this);
        BinOpType opType;

        switch(binaryExpr.exprType) {
            case BinaryExprType.ADD: opType = BinOpType.ADD; break;
            case BinaryExprType.SUB: opType = BinOpType.SUB; break;
            case BinaryExprType.MULT: opType = BinOpType.MUL; break;
            case BinaryExprType.DIV: opType = BinOpType.DIV; break;
            case BinaryExprType.MOD: opType = BinOpType.MOD; break;
            case BinaryExprType.EQUAL: opType = BinOpType.EQ; break;
            case BinaryExprType.NOTEQ: opType = BinOpType.NEQ; break;
            case BinaryExprType.LT: opType = BinOpType.LT; break;
            case BinaryExprType.LEQ: opType = BinOpType.LEQ; break;
            case BinaryExprType.GEQ: opType = BinOpType.GEQ; break;
            case BinaryExprType.GT: opType = BinOpType.GT; break;
            case BinaryExprType.OR: opType = BinOpType.OR; break;
            case BinaryExprType.AND: opType = BinOpType.AND; break;
            default:
                throw new Exception(
                    String.Format(
                        "IRGenerator: {0} is not a supported binary operator for IR generation. Use control flow instead.", 
                        binaryExpr.exprType
                    )
                );
        }

        return matchThenReturn<T, IRBinOp>(
            new IRBinOp(
                opType, irLeft, irRight
            )
        );
    }

    public T visit<T>(UnaryExprAST unaryExpr) { 
        IRExpr irOperand = unaryExpr.operand.accept<IRExpr>(this);
        UnaryOpType opType;
        switch(unaryExpr.exprType) {
            case UnaryExprType.NEGATE: opType = UnaryOpType.NEGATE; break;
            case UnaryExprType.NOT: opType = UnaryOpType.NOT; break;
            default:
                throw new Exception(
                    String.Format(
                        "IRGenerator: {0} is not supported as a unary operation.",
                        unaryExpr.exprType
                    )
                );
        }

        return matchThenReturn<T, IRUnaryOp>(
            new IRUnaryOp(
                opType, irOperand
            )
        );
    }
    
    public T visit<T>(VarAccessAST varAccess) { 
        IRTemp temp = new IRTemp(varAccess.variableName);
        return matchThenReturn<T, IRTemp>(temp);
    }

    public T visit<T>(ArrayAccessAST arrayAccess) { 
        IRTemp tA = new IRTemp(String.Format("{0}A", arrayAccess.arrayName));
        IRTemp tI = new IRTemp(String.Format("{0}I", arrayAccess.arrayName));

        //Step 1: Moving the name of the array into a new register (tA).
        IRMove regStartAddr = new IRMove(
            tA,
            new IRTemp(arrayAccess.arrayName)
        );

        //Step 2: Computing the index, then moving it into the tI register.
        IRExpr arrayIndexAddr = arrayAccess.accessValue.accept<IRExpr>(this);
        IRMove regIndexAddr = new IRMove(
            tI,
            arrayIndexAddr
        );
        

        //Step 3: Comparing tI with the length of the array, which lives at tA-4 in memory(wordSize = 4)

        //Jumping to the out of bounds error is there is a problem.
        //NOTE: unsigned less than is used to handle negative indexes, because
        //this is result in a large number. Neat trick.
        IRLabel okLabel = createNewLabel();
        IRCJump determineOutOfBounds = new IRCJump(
            new IRBinOp(
                BinOpType.ULT,
                tI,
                new IRMem(
                    MemType.NORMAL,
                    new IRBinOp(
                        BinOpType.SUB,
                        tA,
                        new IRConst(IRConfiguration.wordSize)
                    )
                )
            ),
            okLabel.name,
            IRConfiguration.OUT_OF_BOUNDS_FLAG
        );

        IRSeq computeAccessAddr = new IRSeq(new List<IRStmt>() {
            regStartAddr,
            regIndexAddr,
            determineOutOfBounds,
            okLabel
        });

        // Finally dereferencing by Mem[tA + 4*tI] 
        IRMem dereferenceAccessAddr = new IRMem(
            MemType.NORMAL,
            new IRBinOp(
                BinOpType.ADD,
                tA,
                new IRBinOp(
                    BinOpType.MUL,
                    tI,
                    new IRConst(IRConfiguration.wordSize)
                )
            )
        );

        IR_Eseq evalThenExe = new IR_Eseq(
            computeAccessAddr,
            dereferenceAccessAddr
        );
        
        return matchThenReturn<T, IR_Eseq>(evalThenExe);
    }

    //TODO: Implement this later.
    public T visit<T>(MultiDimArrayAccessAST multiDimArrayAccess) { throw new NotImplementedException(); }

    public T visit<T>(FunctionCallAST functionCall) { 
        List<IRExpr> irFuncArgs = new List<IRExpr>();
        foreach(ExprAST funcArgAST in functionCall.args) {
            IRExpr irFuncArg = funcArgAST.accept<IRExpr>(this);
            irFuncArgs.Add(irFuncArg);
        }

        IRCall irCall = new IRCall(
            new IRName(functionCall.functionName),
            irFuncArgs
        );

        return matchThenReturn<T, IRCall>(irCall);
    }

    public T visit<T>(IntLiteralAST intLiteral) { 
        IRConst irConst = new IRConst(intLiteral.value);
        return matchThenReturn<T, IRConst>(irConst);
    }

    public T visit<T>(BoolLiteralAST boolLiteral) { 
        IRConst irConst = new IRConst(boolLiteral.value ? 1 : 0);
        return matchThenReturn<T, IRConst>(irConst);
    }

    public T visit<T>(CharLiteralAST charLiteral) { 
        IRConst irConst = new IRConst(charLiteral.asciiValue);
        return matchThenReturn<T, IRConst>(irConst);
    }

    public T visit<T>(StrLiteralAST strLiteral) { throw new NotImplementedException(); }

    private ExpectedType matchThenReturn<ExpectedType, ActualType>(ActualType type) {
        if (type is ExpectedType specifiedType) {
            return specifiedType;
        } else {
            throw new Exception(
                String.Format(
                    "IRGenerator: Expected {0}, but got {1}", 
                    typeof(ExpectedType).ToString(),
                    typeof(ActualType).ToString()
                )
            );
        }
    }

    private T lookUpSymbolFromContext<T>(string identifier, int lineNum, int colNum) where T : Symbol {
        T? symbol = context.lookup<T>(identifier);
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

    private IRLabel createNewLabel() {
        IRLabel label = new IRLabel(String.Format(
            "L{0}:", labelCounter.ToString()
        ));
        labelCounter++;

        return label;
    }

    /*
     * Control flow is used for generating IR for expressions that output booleans.
     */
    private IRStmt translateBoolExprByCF(ExprAST expr, IRLabel trueLabel, IRLabel falseLabel) {
        switch(expr) {
            case BinaryExprAST binExpr:
                return matchBinaryExprBool(binExpr, trueLabel, falseLabel);
            case UnaryExprAST unaryExpr:
                return matchUnaryExprBool(unaryExpr, trueLabel, falseLabel);
            case BoolLiteralAST boolLit:
                return matchBoolLiteral(boolLit, trueLabel, falseLabel);
            default:
                IRExpr irExpr = expr.accept<IRExpr>(this);
                IRCJump condJump = new IRCJump(
                    irExpr, trueLabel.name, falseLabel.name
                );
                return condJump;
        }
    }

    private IRStmt matchBinaryExprBool(BinaryExprAST binExpr, IRLabel trueLabel, IRLabel falseLabel) {
        switch(binExpr.exprType) {
            case BinaryExprType.AND:
                IRLabel L1 = createNewLabel();
                IRStmt leftControlFlow_AND = translateBoolExprByCF(
                    binExpr.leftOperand, L1, falseLabel
                );
                IRStmt rightControlFlow_AND = translateBoolExprByCF(
                    binExpr.rightOperand, trueLabel, falseLabel
                );
                return new IRSeq(new List<IRStmt>() {
                    leftControlFlow_AND,
                    L1,
                    rightControlFlow_AND
                });
            case BinaryExprType.OR:
                IRLabel L2 = createNewLabel();
                IRStmt leftControlFlow_OR = translateBoolExprByCF(
                    binExpr.leftOperand, L2, trueLabel
                );
                IRStmt rightControlFlow_OR = translateBoolExprByCF(
                    binExpr.rightOperand, trueLabel, falseLabel
                );
                return new IRSeq(new List<IRStmt>() {
                    leftControlFlow_OR,
                    L2,
                    rightControlFlow_OR
                });
            default:
                IRBinOp binOp = binExpr.accept<IRBinOp>(this);

                return new IRCJump(
                    binOp,
                    trueLabel.name,
                    falseLabel.name
                );
        }
    }

    private IRStmt matchUnaryExprBool(UnaryExprAST unaryExpr, IRLabel trueLabel, IRLabel falseLabel) {
        switch(unaryExpr.exprType) {
            case UnaryExprType.NOT:
                return translateBoolExprByCF(
                    unaryExpr.operand,
                    falseLabel,
                    trueLabel
                );
            default:
                IRUnaryOp unaryOp = unaryExpr.accept<IRUnaryOp>(this);
                return new IRCJump(
                    unaryOp, 
                    trueLabel.name, 
                    falseLabel.name
                );
        }
        throw new Exception();
    }

    private IRStmt matchBoolLiteral(BoolLiteralAST boolLit, IRLabel trueLabel, IRLabel falseLabel) {
        switch(boolLit.value) {
            case true:
                return new IRJump(
                    new IRName(trueLabel.name)
                );
            case false:
                return new IRJump(
                    new IRName(falseLabel.name)
                );
        }
    }
}