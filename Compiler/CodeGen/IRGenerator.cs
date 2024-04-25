using System.Reflection.Emit;
using CompilerProj.Context;
using CompilerProj.Visitors;

/*
 * This is where the fun begins.
 * This visitor will emit the middle level intermediate representation.
 */
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

    /*
     * S[x: t = e] = MOVE(TEMP(x), E(e))
     */
    public T visit<T>(VarDeclAST varDecl) { 
        IRExpr srcExpr;
        if (varDecl.initialValue == null) {
            srcExpr = new IRConst(0);
        } else {
            srcExpr = varDecl.initialValue.accept<IRExpr>(this);
        }

        IRMove irMove = new IRMove(
            new IRTemp(varDecl.name),
            srcExpr
        );

        return matchThenReturn<T, IRMove>(irMove);
    }

    /*
     * S[x1: t1, x2: t2,...,xn: tn = e1, e2,...,en] = SEQ(
     *    MOVE(TEMP(x1), E(e1)),
     *    MOVE(TEMP(x2), E(e2)),
     *    .
     *    .
     *    .
     *    MOVE(TEMP(xn), E(en)),
     * )
     */
    public T visit<T>(MultiVarDeclAST multiVarDecl) { 
        List<IRStmt> irStmts = new List<IRStmt>();

        foreach(KeyValuePair<string, ExprAST?> varWithInitVal in multiVarDecl.initialValues) {
            string varDecl = varWithInitVal.Key;
            ExprAST? initialVal = varWithInitVal.Value;
            IRExpr irInitial;

            if (initialVal == null) {
                irInitial = new IRConst(0);
            } else {
                irInitial = initialVal.accept<IRExpr>(this);
            }

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

    /*
     * S[x1: t1, x2: t2,...,xn: tn = function(e1, e2,...,en)] = SEQ(
     *   CALLStmt(NAME(function), E[e1], E[e2],...,E[en]),
     *   MOVE(TEMP(x1), RET1),
     *   MOVE(TEMP(x2), RET2),
     *   .
     *   .
     *   .
     *   MOVE(TEMP(xn), RETn),
     * )
     */
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

    /*
     * Either S[x: t[e]] or S[x: t[] = {e1, e2,...,en}]
     */
    public T visit<T>(ArrayDeclAST array) {
        if (array.initialValues == null) {
            Tuple<List<IRTemp>, IRSeq> regsAndIR = allocateArrayDecl_WithoutExpr(array.name, array.size);
            return matchThenReturn<T, IRSeq>(
                regsAndIR.Item2
            );

        } else {
            Tuple<List<IRTemp>, IR_Eseq> regsAndIR = allocateArrayDecl_WithExpr(
                array.name,
                array.initialValues
            );

            IRTemp tArray = regsAndIR.Item1[1];
            IRMove moveFinalAddr_Into_tArray = new IRMove(
                tArray,
                regsAndIR.Item2
            );
            return matchThenReturn<T, IRSeq>(
                new IRSeq(new List<IRStmt>() {moveFinalAddr_Into_tArray})
            );
        }
    }

    /*
     * S[x: t[e]] = SEQ (
     *   MOVE(tSize, E[e]),
     *   MOVE(tArrayAddr, Call(Name("malloc"), tSize*wordSize + wordSize)), NOTE: wordSize = 4)
     *   MOVE(MEM(tArrayAddr), tSize)
     *   MOVE(TEMP(x), tArrayAddr + wordSize)
     * )
     * 
     * NOTE:
     * Since this is a reused function, tSize, tArrayAddr, and TEMP(x) are explicited
     * returned as well.
     */
    private Tuple<List<IRTemp>, IRSeq> allocateArrayDecl_WithoutExpr(string arrayName, ExprAST arraySize) {
        /** Register that holds the array size **/
        IRTemp tSize = new IRTemp(String.Format("{0}Size", arrayName));

        /** Register that holds starting address for the entire array INCLUDING THE SIZE. **/
        IRTemp tArrayAddr = new IRTemp(String.Format("{0}Addr", arrayName));

        /** Register that holds the starting address for indexing the array **/
        IRTemp tArray = new IRTemp(arrayName);

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
            tArrayAddr,
            irCallMalloc
        );

        //Step 3: Using "tArrayAddr", place the size at that address in memory.
        IRMove storeSizeInMem = new IRMove(
            new IRMem(MemType.NORMAL, tArrayAddr),
            tSize
        );

        //Step 4: A register named after the array will now hold the starting address for the array.
        IRMove createArrayRegister = new IRMove(
            tArray,
            new IRBinOp(
                BinOpType.ADD,
                tArrayAddr,
                new IRConst(IRConfiguration.wordSize)
            )
        );

        return Tuple.Create<List<IRTemp>, IRSeq>(
            new List<IRTemp>() {
                tSize,
                tArrayAddr,
                tArray
            },
            new IRSeq(new List<IRStmt>() {
                computeSize,
                allocateMem,
                storeSizeInMem,
                createArrayRegister
            })
        );
    }
    
    /*
     * S[x: t[] = {e1, e2,...,en}] = ESEQ(
     *   SEQ(
     *       MOVE(tArrayAddr, Call(Name("malloc"), n*wordSize + wordSize)), NOTE: wordSize=4
     *       MOVE(MEM(tArrayAddr), CONST(n))
     *       MOVE(MEM(tArrayAddr + wordSize), E[e1])
     *       MOVE(MEM(tArrayAddr + wordSize*2), E[e2])
     *       .
     *       .
     *       .
     *       MOVE(MEM(tArrayAddr + wordSize*n), E[en])
     *   ),
     *   ADD(tArrayAddr, wordSize)
     * )
     */
    private Tuple<List<IRTemp>, IR_Eseq> allocateArrayDecl_WithExpr(string arrayName, ExprAST[] initialValues) {
        IRTemp tArrayAddr = new IRTemp(
            String.Format(
                "{0}A", arrayName
            )
        );
        IRTemp tArray = new IRTemp(arrayName);

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
        IRMove moveMallocIntoArrayAddr = new IRMove(
            tArrayAddr, callMalloc
        );
        IRMove move_Length_Into_DereferencedArrayAddr = new IRMove(
            new IRMem(MemType.NORMAL, tArrayAddr),
            new IRConst(initialValues.Length)
        );
        allMoves.Add(moveMallocIntoArrayAddr);
        allMoves.Add(move_Length_Into_DereferencedArrayAddr);

        for (int i = 0; i < initialValues.Length; i++) {
            IRMem dereferencedOffsetFromArrayAddr = new IRMem(
                MemType.NORMAL, new IRBinOp(
                    BinOpType.ADD,
                    tArrayAddr,
                    new IRConst((i+1)*IRConfiguration.wordSize)
                )
            );
            IRExpr initialValue = initialValues[i].accept<IRExpr>(this);

            IRMove move_InitVal_Into_DereferencedOffsetFromArrayAddr = new IRMove(
                dereferencedOffsetFromArrayAddr,
                initialValue
            );
            allMoves.Add(
                move_InitVal_Into_DereferencedOffsetFromArrayAddr
            );
        }
        

        return Tuple.Create<List<IRTemp>, IR_Eseq>(
            new List<IRTemp>() {
                 tArrayAddr,
                 tArray
            },
            new IR_Eseq(
                new IRSeq(allMoves),
                new IRBinOp(
                    BinOpType.ADD,
                    tArrayAddr, new IRConst(IRConfiguration.wordSize)
                )
            )
        );
    }

    public T visit<T>(MultiDimArrayDeclAST multiDimArray) { 
        if (multiDimArray.initialValues == null) {
            return matchThenReturn<T, IRSeq>(
                allocateMultiDimArray_WithoutExprs(
                    multiDimArray.name,
                    multiDimArray.rowSize,
                    multiDimArray.colSize
                )
            );
        } else {
            return matchThenReturn<T, IRSeq>(
                allocateMultiDimArray_WithExprs(
                    multiDimArray.name, 
                    multiDimArray.initialValues
                )
            );
        }
        throw new NotImplementedException(); 
    }
    
    /*
     * S[array: t[e1][e2]] = SEQ(
     *      S[array: t[e1]], //access to tRowSize, tArrayAddr_Base too
     *      MOVE(tI, 0),
     *      LABEL(lH),
     *      C[tI < tRowSize, t, f], 
     *      LABEL(t),
     *      S[array: t[e2]], //access to tArrayLocation
     *      MOVE(MEM( (tArrayAddr_Base+wordSize) + (tI*wordSize)), tArrayLocation) ,
     *      MOVE(tI, Add(tI, 1)),
     *      JUMP(lH),
     *      LABEL(f)
     * )
     */
    private IRSeq allocateMultiDimArray_WithoutExprs(string arrayName, ExprAST rowSize, ExprAST colSize) {
        /// Allocating space for the array rows ///
        Tuple<List<IRTemp>, IRSeq> allocateArrayRows_WithRegs = allocateArrayDecl_WithoutExpr(
            arrayName,
            rowSize
        ); 
        IRTemp tRowSize = allocateArrayRows_WithRegs.Item1[0];
        IRTemp tArrayAddr_Base = allocateArrayRows_WithRegs.Item1[1];
        List<IRStmt> allStmts = allocateArrayRows_WithRegs.Item2.statements;

        /// Setting up the loop for allocating space for the columns///
        IRTemp tI = new IRTemp(String.Format("{0}RowI", arrayName));
        IRMove initialize_TI_To_0 = new IRMove(tI, new IRConst(0));
        IRLabel startLoopLabel = createNewLabel();
        IRLabel trueLoopLabel = createNewLabel();
        IRLabel falseLoopLabel = createNewLabel();
        IRCJump loopCondition = new IRCJump(
             new IRBinOp(BinOpType.LT, tI, tRowSize),
             trueLoopLabel.name,
             falseLoopLabel.name
        );
        allStmts.Add(initialize_TI_To_0);
        allStmts.Add(startLoopLabel);
        allStmts.Add(loopCondition);
        allStmts.Add(trueLoopLabel);

        /// Allocating space for the array columns, wrapped in a loop: while(i < row) ///
        Tuple<List<IRTemp>, IRSeq> allocateArrayCols_WithRegs = allocateArrayDecl_WithoutExpr(
            arrayName + "Col",
            colSize
        ); 
        IRTemp tArrayLocation = allocateArrayCols_WithRegs.Item1[2];
        allStmts = allStmts.Concat(allocateArrayCols_WithRegs.Item2.statements).ToList();
        IRMove pointToArrayLocation = new IRMove(
            new IRMem(MemType.NORMAL, new IRBinOp(
                BinOpType.ADD, 
                new IRBinOp(BinOpType.ADD, tArrayAddr_Base, new IRConst(IRConfiguration.wordSize)), 
                new IRBinOp(BinOpType.MUL, tI, new IRConst(IRConfiguration.wordSize))
            )),
            tArrayLocation
        );
        allStmts.Add(pointToArrayLocation);

        /// Incrementing tI, and closing the while loop ///
        IRMove incrementTI = new IRMove(
            tI,
            new IRBinOp(
                BinOpType.ADD,
                tI, new IRConst(1)
            )
        );
        IRJump jumpToTopOfLoop = new IRJump(
            new IRName(startLoopLabel.name)
        );
        allStmts.Add(incrementTI);
        allStmts.Add(jumpToTopOfLoop);
        allStmts.Add(falseLoopLabel);

        return new IRSeq(allStmts);
    }
    /*
     * S[array: t[][] = {{e11,...,e1n},...,{em1,...,emn}}] = SEQ(
     *      MOVE(tArrayAddr, Call(Name("malloc"), m*wordSize + wordSize)), NOTE: wordSize=4
     *      MOVE(MEM(tArrayAddr), CONST(m))
     *      MOVE(MEM(tArrayAddr + wordSize), E[array: t[] = {e11,,,e1n}])
     *      MOVE(MEM(tArrayAddr + wordSize*2), E[array: t[] = {e21,,,e2n}])
     *      .
     *      .
     *      .
     *      MOVE(MEM(tArrayAddr + wordSize*m), E[array: t[] = {em1,,,emn}]),
     *      MOVE(TEMP(x), tArrayAddr + wordSize)
     * )
     */
    private IRSeq allocateMultiDimArray_WithExprs(string arrayName, ExprAST[][] initialValues) {
        int numRows = initialValues.Length;
        IRTemp tArrayAddr = new IRTemp(
            String.Format(
                "{0}A", arrayName
            )
        );
        IRTemp tArray = new IRTemp(arrayName);

        IRBinOp bytesToAllocate = new IRBinOp(
            BinOpType.ADD,
            new IRBinOp(
                BinOpType.MUL,
                new IRConst(numRows),
                new IRConst(IRConfiguration.wordSize)
            ),
            new IRConst(IRConfiguration.wordSize)
        );
        IRCall callMalloc = new IRCall(
            new IRName("malloc"),
            new List<IRExpr>() { bytesToAllocate }
        );
        IRMove initialize_tArrayAddr = new IRMove(
            tArrayAddr,
            callMalloc
        );
        IRMove move_Length_Into_DereferencedArrayAddr = new IRMove(
            new IRMem(MemType.NORMAL, tArrayAddr),
            new IRConst(numRows)
        );

        List<IRStmt> allMoves = new List<IRStmt>();
        allMoves.Add(initialize_tArrayAddr);
        allMoves.Add(move_Length_Into_DereferencedArrayAddr);

        for (int i = 0; i < numRows; i++) {
            IRMem dereferencedOffsetFromArrayAddr = new IRMem(
                MemType.NORMAL, new IRBinOp(
                    BinOpType.ADD,
                    tArrayAddr,
                    new IRConst((i+1)*IRConfiguration.wordSize)
                )
            );
            Tuple<List<IRTemp>, IR_Eseq> subArrayWithRegs = allocateArrayDecl_WithExpr(
                String.Format("{0}{1}", arrayName, i),
                initialValues[i]
            );
            IR_Eseq allocatingSubArray = subArrayWithRegs.Item2;

            IRMove move_InitVal_Into_DereferencedOffsetFromArrayAddr = new IRMove(
                dereferencedOffsetFromArrayAddr,
                allocatingSubArray
            );
            allMoves.Add(
                move_InitVal_Into_DereferencedOffsetFromArrayAddr
            );
        }
        IRMove createArrayRefReg = new IRMove(
            tArray,
            new IRBinOp(
                BinOpType.ADD,
                tArrayAddr, new IRConst(IRConfiguration.wordSize)
            )
        );
        allMoves.Add(createArrayRefReg);

        return new IRSeq(allMoves);
    }

    /*
     * S[x: t[] = functionCall(p1,...,pn)] = 
     *   IRMOVE(TEMP(x), E[functionCall(p1,...,pn)])
     * 
     * NOTE: Since arrays will be stored on the heap, this is all it takes. Passing the starting address. 
     * NICE!
     */
    //TODO: Implement these
    public T visit<T>(ArrayDeclCallAST arrayCall) {
        
        IRCall irFunctionCall = arrayCall.function.accept<IRCall>(this);
        IRTemp arrayReg = new IRTemp(arrayCall.name);
        return matchThenReturn<T, IRMove>(
            new IRMove(
                arrayReg,
                irFunctionCall
            )
        );
    }

    /*
     * S[x: t[] = functionCall(p1,...,pn)] = 
     *   IRMOVE(TEMP(x), E[functionCall(p1,...,pn)])
     *
     * NOTE: Since arrays will be stored on the heap, this is all it takes. Passing the starting address. 
     * NICE!
     */
    public T visit<T>(MultiDimArrayDeclCallAST multiDimArrayCall) {

        IRCall irFunctionCall = multiDimArrayCall.function.accept<IRCall>(this);
        IRTemp arrayReg = new IRTemp(multiDimArrayCall.name);
        return matchThenReturn<T, IRMove>(
            new IRMove(
                arrayReg,
                irFunctionCall
            )
        );
    }

    /*
     * S[f(x1: t1,...,xn:tn): p1,...,pn Body] = SEQ(
     *   LABEL(f)
     *   S[p1]
     *   .
     *   .
     *   .
     *   S[pn]
     *   S[Body]
     * )
     */
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

    /*
     * S[p] = MOVE(P, Temp(ARG))
     */
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

    /*
     *
     * S[body] = SEQ(
     *   S[s1]
     *   .
     *   .
     *   .
     *   S[sn]  
     * )
     */
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

    /*
     * S[x = e] = MOVE(TEMP(x), E(e))
     */
    public T visit<T>(AssignAST assign) { 
        IRExpr irSrc = assign.value.accept<IRExpr>(this);
        IRTemp irDest = assign.variable.accept<IRTemp>(this);

        IRMove irAssign = new IRMove(
            irDest, irSrc
        );

        return matchThenReturn<T, IRMove>(irAssign);
    }

    /*
     * S[x1, x2,...,xn = e1, e2,...,en] = SEQ(
     *    MOVE(TEMP(x1), E(e1)),
     *    MOVE(TEMP(x2), E(e2)),
     *    .
     *    .
     *    .
     *    MOVE(TEMP(xn), E(en)),
     * )
     */
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

    /*
     * S[x1, x2,...,xn = function(e1, e2,...,en)] = SEQ(
     *   CALLStmt(NAME(function), E[e1], E[e2],...,E[en]),
     *   MOVE(TEMP(x1), RET1),
     *   MOVE(TEMP(x2), RET2),
     *   .
     *   .
     *   .
     *   MOVE(TEMP(xn), RETn),
     * )
     */
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
    
    /*
     * S[x[e1] = e2] = SEQ(
     *   E[x[e1]],
     *   MOVE(MEM(tA + tI * wordSize), E[e2])
     * ) 
     */
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

        // Mem[tA + wordSize*tI] <- assignValue
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

    /*
     * S[array[e1][e2] = e3] = SEQ(
     *   S[array[e1][e2]],
     *   MOVE(MEM(tArray + tE2 * wordSize), E[e3])
     * )
     */
    public T visit<T>(MultiDimArrayAssignAST multiDimArrayAssign) { 
        IR_Eseq computeThenDeref_accessAddr = multiDimArrayAssign.arrayAccess.accept<IR_Eseq>(this);
        if (computeThenDeref_accessAddr.stmt.GetType() != typeof(IRSeq)) {
            throw new Exception("Computation of the access access was expected to be an IRSeq");
        }

        if (computeThenDeref_accessAddr.expr.GetType() != typeof(IRMem)) {
            throw new Exception("Dereference of the access access was expected to be an IRMem");
        }

        IRSeq computeAccessAddr = (IRSeq) computeThenDeref_accessAddr.stmt;
        IRMem derefAccessAddr = (IRMem) computeThenDeref_accessAddr.expr;
        IRExpr assignValue = multiDimArrayAssign.value.accept<IRExpr>(this);

        // Mem[tArray + wordSize*tE2] <- assignValue
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

    public T visit<T>(ConditionalAST conditional) { 
        IRSeq seqStmts;
        if (conditional.elseIfConditionalBlocks == null || conditional.elseIfConditionalBlocks.Count == 0) {
            if (conditional.elseBlock == null) {
                seqStmts = generateIfStmt(
                    conditional.ifCondition, conditional.ifBlock
                );
            } else {
                seqStmts = generateIfElseStmt(
                    conditional.ifCondition, conditional.ifBlock, conditional.elseBlock
                );
            }
        } else {
            if (conditional.elseBlock == null) {
                seqStmts = generateIf_ElseIf_Stmt(
                    conditional.ifCondition,
                    conditional.ifBlock,
                    conditional.elseIfConditionalBlocks
                );

            } else {
                seqStmts = generateIf_ElseIf_ElseStmt(
                    conditional.ifCondition,
                    conditional.ifBlock,
                    conditional.elseIfConditionalBlocks,
                    conditional.elseBlock
                );
            }
        }

        return matchThenReturn<T, IRSeq>(seqStmts);
    }

    private IRSeq generateIfStmt(ExprAST ifCondition, BlockAST ifBlock) {
        IRLabel trueLabel = createNewLabel();
        IRLabel falseLabel = createNewLabel();

        IRStmt irIfCondition = translateBoolExprByCF(
            ifCondition, trueLabel, falseLabel
        );
        IRSeq irIfBlock = ifBlock.accept<IRSeq>(this);

        return new IRSeq(new List<IRStmt>() {
            irIfCondition,
            trueLabel,
            irIfBlock,
            falseLabel
        });
    }

    private IRSeq generateIfElseStmt(ExprAST ifCondition, BlockAST ifBlock, BlockAST elseBlock) {
        IRSeq irIfStmt = generateIfStmt(ifCondition, ifBlock);
        List<IRStmt> stmts = irIfStmt.statements;

        IRSeq irElseBlock = elseBlock.accept<IRSeq>(this);
        stmts.Add(irElseBlock);

        return new IRSeq(stmts);
    }

    private IRSeq generateIf_ElseIf_Stmt(
        ExprAST ifCondition, BlockAST ifBlock, 
        Dictionary<ExprAST, BlockAST> elseIfConditionalBlocks
    ) {
        IRLabel ifTrueLabel = createNewLabel();
        IRLabel ifFalseLabel = createNewLabel();

        List<IRStmt> stmts = new List<IRStmt>();

        IRStmt irIfCondition = translateBoolExprByCF(
            ifCondition, ifTrueLabel, ifFalseLabel
        );
        IRSeq irIfBlock = ifBlock.accept<IRSeq>(this);
        stmts.Add(irIfCondition);
        stmts.Add(ifTrueLabel);
        stmts.Add(irIfBlock);
        stmts.Add(ifFalseLabel);

        foreach(KeyValuePair<ExprAST, BlockAST> elseIfCondBlock in elseIfConditionalBlocks) {
            ExprAST elseIfCondition = elseIfCondBlock.Key;
            BlockAST elseIfBlock = elseIfCondBlock.Value;

            IRLabel elseIfTrueLabel = createNewLabel();
            IRLabel elseIfFalseLabel = createNewLabel();
            IRStmt irElseIfCond = translateBoolExprByCF(
                elseIfCondition, elseIfTrueLabel, elseIfFalseLabel
            );
            IRSeq irElseIfBlock = elseIfBlock.accept<IRSeq>(this);
            stmts.Add(irElseIfCond);
            stmts.Add(elseIfTrueLabel);
            stmts.Add(irElseIfBlock);
            stmts.Add(elseIfFalseLabel);
        }

        return new IRSeq(stmts);
    }

    private IRSeq generateIf_ElseIf_ElseStmt(
        ExprAST ifCondition, BlockAST ifBlock, 
        Dictionary<ExprAST, BlockAST> elseIfConditionalBlocks,
        BlockAST elseBlock
    ) {
        IRSeq irIf_ElseIf_Stmts = generateIf_ElseIf_Stmt(
            ifCondition, ifBlock,
            elseIfConditionalBlocks
        );

        List<IRStmt> irStmts = irIf_ElseIf_Stmts.statements;
        IRSeq irElseBlock = elseBlock.accept<IRSeq>(this);
        irStmts.Add(irElseBlock);

        return new IRSeq(irStmts);
    }

    /*
     * S[while (e) s] = SEQ(
     *   Label(lH),
     *   C[e, true, false],
     *   Label(true),
     *   S[s],
     *   JUMP(lH),
     *   Label(false)
     * )
     */
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

    /*
     * S[return(e1,...,en)] = Return(E[e1],...,E[en])
     */
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

    /*
     * S[procedure(e1,...,en)] = CallStmt(Name(procedure), E[e1],...,E[en])
     */
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
    
    /*
     * E[e1 OP e2] = BinOP(OP, E[e1], E[e2])
     */
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

    /*
     * E[OP e] = UnaryOp(OP, E[e])
     */
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
    
    /*
     * E[x] = TEMP(x)
     */
    public T visit<T>(VarAccessAST varAccess) { 
        IRTemp temp = new IRTemp(varAccess.variableName);
        return matchThenReturn<T, IRTemp>(temp);
    }

    /*
     * E[array[e2]] = ESEQ(
     *   SEQ(
     *       MOVE(tArray, TEMP(array)),
     *       MOVE(tI, E[e2])
     *       CJUMP(ULT (tI, MEM(tArray - wordSize)), trueLabel, outOfBoundsLabel),
     *       Label(trueLabel)
     *   ),
     *   MEM(tArray + tI * wordSize)
     * )
     */
    public T visit<T>(ArrayAccessAST arrayAccess) { 
        IRTemp tArray = new IRTemp(arrayAccess.arrayName);
        IRTemp tI = new IRTemp(String.Format("{0}I", arrayAccess.arrayName));

        //Step 1: Computing the index, then moving it into the tI register.
        IRExpr arrayIndexAddr = arrayAccess.accessValue.accept<IRExpr>(this);
        IRMove regIndexAddr = new IRMove(
            tI,
            arrayIndexAddr
        );
        

        //Step 2: Comparing tI with the length of the array, which lives at tA-4 in memory(wordSize = 4)

        //Jumping to the out of bounds error when there is a problem.
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
                        tArray,
                        new IRConst(IRConfiguration.wordSize)
                    )
                )
            ),
            okLabel.name,
            IRConfiguration.OUT_OF_BOUNDS_FLAG
        );

        IRSeq computeAccessAddr = new IRSeq(new List<IRStmt>() {
            regIndexAddr,
            determineOutOfBounds,
            okLabel
        });

        // Finally dereferencing by Mem[tA + 4*tI] 
        IRMem dereferenceAccessAddr = new IRMem(
            MemType.NORMAL,
            new IRBinOp(
                BinOpType.ADD,
                tArray,
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

    /*
     * E[array[e1][e2]] = ESEQ(SEQ(
     *      MOVE(tName, TEMP(array)),
     *      MOVE(tE1, E[e1]),
     *      CJUMP(ULT(tE1, MEM(tName - 4), ok1, outOfBounds)),
     *      LABEL(ok1),
     *
     *      MOVE(tArray, MEM(tName + tE1*wordSize)),
     *      MOVE(tE2, E[e2])
     *      CJUMP(ULT(tE2, MEM(tArray - 4), ok2, outOfBounds)),
     *      LABEL(ok2),   
     *   ),
     *   MEM(tArray + tE2*wordSize) //wordSize = 4
     * )
     */
    public T visit<T>(MultiDimArrayAccessAST multiDimArrayAccess) {
        string arrayName = multiDimArrayAccess.arrayName;
        IRTemp tName = new IRTemp(arrayName);
        IRTemp tE1 = new IRTemp(String.Format("{0}E1", arrayName));
        IRTemp tE2 = new IRTemp(String.Format("{0}E2", arrayName));
        IRTemp tArray = new IRTemp(String.Format("{0}A", arrayName));
        
        IRLabel firstOkLabel = createNewLabel();
        IRLabel secondOkLabel = createNewLabel();

        //Step 1: Computing e1.
        IRMove initializeRowIndex = new IRMove(
            tE1, 
            multiDimArrayAccess.firstIndex.accept<IRExpr>(this)
        );

        //Step2: Making the row index out of bounds check.
        IRCJump rowCheck = new IRCJump(
            new IRBinOp(
                BinOpType.ULT,
                tE1,
                new IRMem(
                    MemType.NORMAL,
                    new IRBinOp(
                        BinOpType.SUB,
                        tName,
                        new IRConst(IRConfiguration.wordSize)
                    )
                )
            ),
            firstOkLabel.name,
            IRConfiguration.OUT_OF_BOUNDS_FLAG
        );

        //Step 3: Moving the row array address into 'tArray'
        IRMove computeRowAddr = new IRMove(
            tArray,
            new IRMem(
                MemType.NORMAL, 
                new IRBinOp(
                    BinOpType.ADD,
                    tName,
                    new IRBinOp(
                        BinOpType.MUL,
                        tE1,
                        new IRConst(IRConfiguration.wordSize)
                    )
                )
            )
        );

        // Step 4: Computing e2
        IRMove initializeColIndex = new IRMove(
            tE2,
            multiDimArrayAccess.secondIndex.accept<IRExpr>(this)
        );

        // Step 5: Making the column index out of bounds check.
        IRCJump colCheck = new IRCJump(
            new IRBinOp(
                BinOpType.ULT,
                tE2,
                new IRMem(
                    MemType.NORMAL,
                    new IRBinOp(
                        BinOpType.SUB,
                        tArray,
                        new IRConst(IRConfiguration.wordSize)
                    )
                )
            ),
            secondOkLabel.name,
            IRConfiguration.OUT_OF_BOUNDS_FLAG
        );

        IRSeq computeAccessAddr = new IRSeq(new List<IRStmt>() {
            initializeRowIndex,
            rowCheck,
            firstOkLabel,
            computeRowAddr,
            initializeColIndex,
            colCheck,
            secondOkLabel
        });

        // Finally dereferencing by Mem[tArray + wordSize*tE2] 
        IRMem dereferenceAccessAddr = new IRMem(
            MemType.NORMAL,
            new IRBinOp(
                BinOpType.ADD,
                tArray,
                new IRBinOp(
                    BinOpType.MUL,
                    tE2,
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

    /*
     * E[function(p1,...,pn)] = CALL(
     *    NAME(function), E[p1],...,E[pn]
     * )
     */
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

    /*
     * E[n] = CONST(n)
     */
    public T visit<T>(IntLiteralAST intLiteral) { 
        IRConst irConst = new IRConst(intLiteral.value);
        return matchThenReturn<T, IRConst>(irConst);
    }

    /*
     * E[true] = CONST(1)
     * E[false] = CONST(0)
     */
    public T visit<T>(BoolLiteralAST boolLiteral) { 
        IRConst irConst = new IRConst(boolLiteral.value ? 1 : 0);
        return matchThenReturn<T, IRConst>(irConst);
    }

    /*
     * E[c] = CONST(c.asciiValue)
     */
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
     * Translation represented as C[E, t, f]
     * 
     * If there is a boolean expression that needs to utilize short circuiting,
     * then control flow is utilized when emitting IR.
     *
     * Notably happens with statements that have boolean conditions embedded in them.
     * (E.G conditional statements, while loops.)
     */
    private IRStmt translateBoolExprByCF(ExprAST expr, IRLabel trueLabel, IRLabel falseLabel) {
        switch(expr) {
            case BinaryExprAST binExpr:
                return generateBinaryExprBoolByCF(binExpr, trueLabel, falseLabel);
            case UnaryExprAST unaryExpr:
                return generateUnaryExprBoolByCF(unaryExpr, trueLabel, falseLabel);
            case BoolLiteralAST boolLit:
                return generateBoolLiteralByCF(boolLit, trueLabel, falseLabel);
            default:
                IRExpr irExpr = expr.accept<IRExpr>(this);
                IRCJump condJump = new IRCJump(
                    irExpr, trueLabel.name, falseLabel.name
                );
                return condJump;
        }
    }

    /*
     * C[e1&&e2, t, f] = SEQ(C[e1, L1, f], Label(L1), C[e2, t, f])
     * C[e1||e2, t, f] = SEQ(C[e1, L1, t], Label(L1), C[e2, t, f])
     * C[e, t, f] = CJump(E[e], t, f)
     */
    private IRStmt generateBinaryExprBoolByCF(BinaryExprAST binExpr, IRLabel trueLabel, IRLabel falseLabel) {
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

    /*
     * C[!e, t, f] = C[e, f, t]
     */
    private IRStmt generateUnaryExprBoolByCF(UnaryExprAST unaryExpr, IRLabel trueLabel, IRLabel falseLabel) {
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
    }

    /*
     * C[true, t, f] = JUMP(Name(t))
     * C[false, t, f] = JUMP(Name(f))
     */
    private IRStmt generateBoolLiteralByCF(BoolLiteralAST boolLit, IRLabel trueLabel, IRLabel falseLabel) {
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