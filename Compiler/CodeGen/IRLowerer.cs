using CompilerProj.Visitors;

/*
 *  This pass will lower the IR to a more assembly like form.
 *  The lowered IR will have the following properties:
 *
 *  1. No nested sequences of statements in an expression.
 *  2. All instances of E_SEQ are eliminated.
 *     - All statements within E_SEQ are hoisted upon into the statement level.
 *  3. Each statement can only have one side effect 
 *     - All statements on the statement level get linearized
 *  4. All calls are hoisted above onto the statement level, instead
 *     of being nested in an expression.   
 */
public sealed class IRLowerer : IRVisitorGeneric {
    private int tempCounter;

    public IRLowerer(int tempCounter) {
        this.tempCounter = tempCounter;
    }

    public T visit<T>(IRCompUnit compUnit) { 
        Dictionary<string, IRFuncDecl> loweredFuncDecls = new Dictionary<string, IRFuncDecl>();
        foreach(KeyValuePair<string, IRFuncDecl> nameWithFuncDecl in compUnit.functions) {
            string funcName = nameWithFuncDecl.Key;
            IRFuncDecl funcDecl = nameWithFuncDecl.Value;

            IRFuncDecl loweredFuncDecl = funcDecl.accept<IRFuncDecl>(this);
            loweredFuncDecls.Add(funcName, loweredFuncDecl);
        }

        IRCompUnit loweredCompUnit = new IRCompUnit(
            compUnit.name,
            loweredFuncDecls,
            new List<string>() { }
        );
        
        return matchThenReturn<T, IRCompUnit>(loweredCompUnit);
    }

    public T visit<T>(IRFuncDecl funcDecl) { 
        List<IRStmt> loweredBody = funcDecl.body.accept<List<IRStmt>>(this);
        
        IRFuncDecl loweredFuncDecl = new IRFuncDecl(
            funcDecl.name,
            new IRSeq(loweredBody)
        );

        return matchThenReturn<T, IRFuncDecl>(loweredFuncDecl);
    }

    public T visit<T>(IRMove move) { 
        switch(move.target) {
            case IRMem mem:
                IRExprLowered loweredTargetMemContent = mem.expr.accept<IRExprLowered>(this);
                IRExprLowered loweredSrcContent = move.src.accept<IRExprLowered>(this);

                if (commutes(loweredSrcContent.stmts, loweredTargetMemContent.expr)) {
                    List<IRStmt> allLoweredStmts = loweredTargetMemContent.stmts;
                    allLoweredStmts = allLoweredStmts.Concat(loweredSrcContent.stmts).ToList();
                    allLoweredStmts.Add(
                        new IRMove(
                            new IRMem(MemType.NORMAL, loweredTargetMemContent.expr),
                            loweredSrcContent.expr
                        )
                    );

                    return matchThenReturn<T, List<IRStmt>>(allLoweredStmts);

                } else {
                    IRTemp freshTemp = createNewTemp();
                    List<IRStmt> allLoweredStmts = loweredTargetMemContent.stmts;
                    allLoweredStmts.Add(
                        new IRMove(
                            freshTemp,
                            loweredTargetMemContent.expr
                        )
                    );
                    allLoweredStmts = allLoweredStmts.Concat(loweredSrcContent.stmts).ToList();
                    allLoweredStmts.Add(
                        new IRMove(
                            new IRMem(MemType.NORMAL, freshTemp),
                            loweredSrcContent.expr
                        )
                    );

                    return matchThenReturn<T, List<IRStmt>>(allLoweredStmts);
                }
            case IRTemp temp:
                IRExprLowered loweredSrc = move.src.accept<IRExprLowered>(this);
                List<IRStmt> loweredStmts = loweredSrc.stmts;
                loweredStmts.Add(
                    new IRMove(
                        new IRTemp(temp.name),
                        loweredSrc.expr
                    )
                );

                return matchThenReturn<T, List<IRStmt>>(loweredStmts);
            default:
                throw new Exception("Move target must be a temporary or a memory access");
        } 
    }

    public T visit<T>(IRSeq seq) { 
        List<IRStmt> loweredStmts = new List<IRStmt>();

        foreach(IRStmt irStmt in seq.statements) {
            loweredStmts.Add(
                irStmt.accept<IRStmt>(this)
            );
        }

        return matchThenReturn<T, List<IRStmt>>(loweredStmts);
    }

    public T visit<T>(IR_Eseq Eseq) { 
        List<IRStmt> loweredEval = Eseq.stmt.accept<List<IRStmt>>(this);
        IRExprLowered loweredExe = Eseq.expr.accept<IRExprLowered>(this);

        IRExprLowered loweredESeq = new IRExprLowered(
            loweredEval.Concat(loweredExe.stmts).ToList<IRStmt>(),
            loweredExe.expr
        );
        
        return matchThenReturn<T, IRExprLowered>(loweredESeq);
    }

    public T visit<T>(IRCJump cJump) { 
        IRExprLowered loweredCond = cJump.cond.accept<IRExprLowered>(this);
        List<IRStmt> loweredStmts = loweredCond.stmts;

        loweredStmts.Add(
            new IRCJump(
                loweredCond.expr,
                cJump.trueLabel,
                cJump.falseLabel
            )
        );
        
        return matchThenReturn<T, List<IRStmt>>(loweredStmts);
    }

    public T visit<T>(IRExp exp) { 
        IRExprLowered loweredExp = exp.expr.accept<IRExprLowered>(this);
        return matchThenReturn<T, IRExprLowered>(loweredExp);
    }

    public T visit<T>(IRJump jump) { 
        IRExprLowered loweredTarget = jump.target.accept<IRExprLowered>(this);
        
        List<IRStmt> loweredStmts = loweredTarget.stmts;
        loweredStmts.Add(
            new IRJump(loweredTarget.expr)
        );

        return matchThenReturn<T, List<IRStmt>>(loweredStmts);
    }

    public T visit<T>(IRLabel label) { 
        List<IRStmt> loweredStmts = new List<IRStmt>();
        loweredStmts.Add(
            new IRLabel(label.name)
        );

        return matchThenReturn<T, List<IRStmt>>(loweredStmts);
    }

    public T visit<T>(IRReturn Return) { 
        List<IRStmt> loweredStmts = new List<IRStmt>();
        List<IRExpr> loweredReturnTemps = new List<IRExpr>();
        foreach(IRExpr irReturn in Return.returns) {
            IRExprLowered loweredReturn = irReturn.accept<IRExprLowered>(this);
            loweredStmts = loweredStmts.Concat(loweredReturn.stmts).ToList();
            IRTemp newTemp = createNewTemp();

            IRMove moveReturnToNewTemp = new IRMove(
                newTemp,
                loweredReturn.expr
            );
            loweredStmts.Add(moveReturnToNewTemp);
            loweredReturnTemps.Add(newTemp);
        }

        IRReturn loweredReturnStmt = new IRReturn(loweredReturnTemps);
        loweredStmts.Add(loweredReturnStmt);
        
        return matchThenReturn<T, List<IRStmt>>(loweredStmts);
    }

    public T visit<T>(IRCallStmt callStmt) { 
        List<IRStmt> allLoweredStmts = new List<IRStmt>();
        List<IRExpr> loweredArgTemps = new List<IRExpr>();
        foreach(IRExpr irArg in callStmt.args) {
            IRExprLowered loweredArg = irArg.accept<IRExprLowered>(this);
            allLoweredStmts = allLoweredStmts.Concat(loweredArg.stmts).ToList();

            IRTemp newTemp = createNewTemp();
            IRMove moveArgIntoNewTemp = new IRMove(
                newTemp,
                loweredArg.expr
            );
            allLoweredStmts.Add(moveArgIntoNewTemp);
            loweredArgTemps.Add(newTemp);
        }
        IRExprLowered loweredTarget = callStmt.target.accept<IRExprLowered>(this);
        allLoweredStmts = allLoweredStmts.Concat(loweredTarget.stmts).ToList();

        IRTemp newTargetTemp = createNewTemp();
        IRMove moveTargetToNewTemp = new IRMove(
            newTargetTemp,
            loweredTarget.expr
        );
        allLoweredStmts.Add(moveTargetToNewTemp);
        IRCallStmt loweredCall = new IRCallStmt(
            newTargetTemp, loweredArgTemps, callStmt.nReturns
        );
        allLoweredStmts.Add(loweredCall);

        return matchThenReturn<T, List<IRStmt>>(allLoweredStmts);
    }

    public T visit<T>(IRCall call) { 
        List<IRStmt> allLoweredStmts = new List<IRStmt>();
        List<IRExpr> loweredArgTemps = new List<IRExpr>();
        foreach(IRExpr irArg in call.args) {
            IRExprLowered loweredArg = irArg.accept<IRExprLowered>(this);
            allLoweredStmts = allLoweredStmts.Concat(loweredArg.stmts).ToList();

            IRTemp newTemp = createNewTemp();
            IRMove moveArgIntoNewTemp = new IRMove(
                newTemp,
                loweredArg.expr
            );
            allLoweredStmts.Add(moveArgIntoNewTemp);
            loweredArgTemps.Add(newTemp);
        }
        IRExprLowered loweredTarget = call.target.accept<IRExprLowered>(this);
        allLoweredStmts = allLoweredStmts.Concat(loweredTarget.stmts).ToList();

        IRTemp newTargetTemp = createNewTemp();
        IRMove moveTargetToNewTemp = new IRMove(
            newTargetTemp,
            loweredTarget.expr
        );
        allLoweredStmts.Add(moveTargetToNewTemp);
        IRCallStmt loweredCall = new IRCallStmt(
            newTargetTemp, loweredArgTemps, 1
        );
        allLoweredStmts.Add(loweredCall);

        IRTemp newFunctionDestTemp = createNewTemp();
        IRMove moveReturnValIntoFunctionDest = new IRMove(
            new IRTemp(IRConfiguration.ABSTRACT_RET_PREFIX + 1),
            newFunctionDestTemp
        );
        allLoweredStmts.Add(moveReturnValIntoFunctionDest);

        return matchThenReturn<T, IRExprLowered>(
            new IRExprLowered(
                allLoweredStmts,
                newFunctionDestTemp
            )
        ); 
    }
    public T visit<T>(IRBinOp binOp) { 
        IRExprLowered loweredLeft = binOp.left.accept<IRExprLowered>(this);
        IRExprLowered loweredRight = binOp.right.accept<IRExprLowered>(this);

        if (commutes(loweredRight.stmts, loweredLeft.expr)) {
            List<IRStmt> allLoweredStmts = loweredLeft.stmts;
            allLoweredStmts = allLoweredStmts.Concat(loweredRight.stmts).ToList();

            IRBinOp loweredBinOp = new IRBinOp(
                binOp.opType,
                loweredLeft.expr,
                loweredRight.expr
            );

            return matchThenReturn<T, IRExprLowered>(
                new IRExprLowered(
                    allLoweredStmts,
                    loweredBinOp
                )
            );
        } else {
            List<IRStmt> allLoweredStmts = loweredLeft.stmts;
            IRTemp freshTemp = createNewTemp();
            allLoweredStmts.Add(
                new IRMove(
                    freshTemp,
                    loweredLeft.expr
                )
            );
            allLoweredStmts = allLoweredStmts.Concat(loweredRight.stmts).ToList();

            IRBinOp loweredBinOp = new IRBinOp(
                binOp.opType,
                freshTemp,
                loweredRight.expr
            );

            return matchThenReturn<T, IRExprLowered>(
                new IRExprLowered(
                    allLoweredStmts,
                    loweredBinOp
                )
            );
        }
    }

    public T visit<T>(IRUnaryOp unaryOp) { 
        IRExprLowered loweredOperand = unaryOp.operand.accept<IRExprLowered>(this);

        return matchThenReturn<T, IRExprLowered>(new IRExprLowered(
            loweredOperand.stmts,
            new IRUnaryOp(
                unaryOp.opType,
                loweredOperand.expr
            )
        ));
    }

    public T visit<T>(IRConst Const) { 
        IRExprLowered loweredConst = new IRExprLowered(
            new List<IRStmt>(),
            new IRConst(Const.value)
        );
        
        return matchThenReturn<T, IRExprLowered>(loweredConst);
    }

    public T visit<T>(IRMem mem) { 
        IRExprLowered loweredAddress = mem.expr.accept<IRExprLowered>(this);
        IRExprLowered loweredMem = new IRExprLowered(
            loweredAddress.stmts,
            new IRMem(MemType.NORMAL, loweredAddress.expr)
        );
        
        return matchThenReturn<T, IRExprLowered>(loweredMem);
    }

    public T visit<T>(IRName name) { 
        IRExprLowered loweredExpr = new IRExprLowered(
            new List<IRStmt>(),
            new IRName(name.name)
        ); 

        return matchThenReturn<T, IRExprLowered>(loweredExpr);
    }

    public T visit<T>(IRTemp temp) { 
        IRExprLowered loweredTemp = new IRExprLowered(
            new List<IRStmt>(),
            new IRTemp(temp.name)
        );

        return matchThenReturn<T, IRExprLowered>(loweredTemp);
    }

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

    private IRTemp createNewTemp() {
        IRTemp temp = new IRTemp(
            String.Format("t{0}", tempCounter)
        );

        tempCounter++;
        return temp;
    }

    /*
     * Testing if 'loweredStmts' can alter the content of 'loweredExpr' in anyway.
     */
    private bool commutes(List<IRStmt> loweredStmts, IRExpr loweredExpr) {
        if (loweredStmts.Count == 0 || isAnUnaffectedLoweredExpr(loweredExpr)) {
            return true;
        }

        Tuple<List<IRMem>, Dictionary<string, IRTemp>> usedMemsAndTemps = extractPossibleNonCommutes(loweredStmts);
        List<IRMem> usedMems = usedMemsAndTemps.Item1;
        Dictionary<string, IRTemp> usedTemps = usedMemsAndTemps.Item2;

        return hasNoCommutingConflicts(usedMems, usedTemps, loweredExpr);
    }

    private bool isAnUnaffectedLoweredExpr(IRExpr loweredExpr) {
        switch(loweredExpr) {
            case IRName loweredName:
            case IRConst loweredConst:
                return true;
            default:
                return false;
        }
    }

    /*
     * Extracting destinations from the lowered moves that might cause commuting problems.
     */
    private Tuple<List<IRMem>, Dictionary<string, IRTemp>> extractPossibleNonCommutes(List<IRStmt> loweredStmts) {
        List<IRMem> memsUsed = new List<IRMem>();
        Dictionary<string, IRTemp> tempsUsed = new Dictionary<string, IRTemp>();

        foreach(IRStmt loweredStmt in loweredStmts) {
            switch(loweredStmt) {
                case IRMove move:
                    if (move.target.GetType() == typeof(IRTemp)) {
                        IRTemp moveTemp = (IRTemp)move.target;
                        tempsUsed.Add(moveTemp.name, moveTemp);
                    } else if (move.target.GetType() == typeof(IRMem)) {
                        IRMem moveMem = (IRMem)move.target;
                        memsUsed.Add(moveMem);
                    } else {
                        throw new Exception("The move target must be a temp or a mem.");
                    }
                break;
            }
        }

        return Tuple.Create<List<IRMem>, Dictionary<string, IRTemp>>(
            memsUsed,
            tempsUsed
        );
    }

    private bool hasNoCommutingConflicts(
        List<IRMem> usedMems, Dictionary<string, IRTemp> usedTemps, IRExpr expr
    ) {
        switch(expr) {
            case IRMem loweredMem: throw new Exception("Case for handling memory isn't implemented yet.");
            case IRTemp loweredTemp: 
                return !usedTemps.ContainsKey(loweredTemp.name);
            case IRBinOp loweredBinOp:
                return (
                    hasNoCommutingConflicts(usedMems, usedTemps, loweredBinOp.left) 
                    && 
                    hasNoCommutingConflicts(usedMems, usedTemps, loweredBinOp.right)
                );
            case IRUnaryOp loweredUnaryOp:
                return hasNoCommutingConflicts(usedMems, usedTemps, loweredUnaryOp.operand);
            case IRName loweredName:
            case IRConst loweredConst:
                return true;
            default:
                throw new Exception("Something went wrong");
        }
    }

    private struct IRExprLowered {
        public List<IRStmt> stmts;
        public IRExpr expr;

        public IRExprLowered(List<IRStmt> stmts, IRExpr expr) {
            this.stmts = stmts;
            this.expr = expr;
        }
    }
}