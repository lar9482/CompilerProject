using CompilerProj.Visitors;

public sealed class IRLowerer : IRVisitorGeneric {
    private int tempCounter;

    public IRLowerer(int tempCounter) {
        this.tempCounter = tempCounter;
    }

    public T visit<T>(IRCompUnit compUnit) { 
        Dictionary<string, LIRFuncDecl> loweredFuncDecls = new Dictionary<string, LIRFuncDecl>();
        foreach(KeyValuePair<string, IRFuncDecl> nameWithFuncDecl in compUnit.functions) {
            string funcName = nameWithFuncDecl.Key;
            IRFuncDecl funcDecl = nameWithFuncDecl.Value;

            LIRFuncDecl loweredFuncDecl = funcDecl.accept<LIRFuncDecl>(this);
            loweredFuncDecls.Add(funcName, loweredFuncDecl);
        }

        LIRCompUnit loweredCompUnit = new LIRCompUnit(
            compUnit.name,
            loweredFuncDecls
        );
        
        return matchThenReturn<T, LIRCompUnit>(loweredCompUnit);
    }

    public T visit<T>(IRFuncDecl funcDecl) { 
        List<LIRStmt> loweredBody = funcDecl.body.accept<List<LIRStmt>>(this);
        
        LIRFuncDecl loweredFuncDecl = new LIRFuncDecl(
            funcDecl.name,
            loweredBody
        );

        return matchThenReturn<T, LIRFuncDecl>(loweredFuncDecl);
    }

    public T visit<T>(IRMove move) { 
        switch(move.target) {
            case IRMem mem:
                IRExprLowered loweredTargetMemContent = mem.expr.accept<IRExprLowered>(this);
                IRExprLowered loweredSrcContent = move.src.accept<IRExprLowered>(this);

                if (commutes(loweredSrcContent.stmts, loweredTargetMemContent.expr)) {
                    List<LIRStmt> allLoweredStmts = loweredTargetMemContent.stmts;
                    allLoweredStmts = allLoweredStmts.Concat(loweredSrcContent.stmts).ToList();
                    allLoweredStmts.Add(
                        new LIRMoveMem(
                            loweredSrcContent.expr,
                            new LIRMem(loweredTargetMemContent.expr)
                        )
                    );

                    return matchThenReturn<T, List<LIRStmt>>(allLoweredStmts);

                } else {
                    LIRTemp freshTemp = createNewTemp();
                    List<LIRStmt> allLoweredStmts = loweredTargetMemContent.stmts;
                    allLoweredStmts.Add(
                        new LIRMoveTemp(
                            loweredTargetMemContent.expr,
                            freshTemp
                        )
                    );
                    allLoweredStmts = allLoweredStmts.Concat(loweredSrcContent.stmts).ToList();
                    allLoweredStmts.Add(
                        new LIRMoveMem(
                            loweredSrcContent.expr,
                            new LIRMem(freshTemp)
                        )
                    );

                    return matchThenReturn<T, List<LIRStmt>>(allLoweredStmts);
                }
            case IRTemp temp:
                IRExprLowered loweredSrc = move.src.accept<IRExprLowered>(this);
                List<LIRStmt> loweredStmts = loweredSrc.stmts;
                loweredStmts.Add(
                    new LIRMoveTemp(
                        loweredSrc.expr,
                        new LIRTemp(temp.name)
                    )
                );

                return matchThenReturn<T, List<LIRStmt>>(loweredStmts);
            default:
                throw new Exception("Move target must be a temporary or a memory access");
        } 
    }

    public T visit<T>(IRSeq seq) { 
        List<LIRStmt> loweredStmts = new List<LIRStmt>();

        foreach(IRStmt irStmt in seq.statements) {
            loweredStmts.Add(
                irStmt.accept<LIRStmt>(this)
            );
        }

        return matchThenReturn<T, List<LIRStmt>>(loweredStmts);
    }

    public T visit<T>(IR_Eseq Eseq) { 
        List<LIRStmt> loweredEval = Eseq.stmt.accept<List<LIRStmt>>(this);
        IRExprLowered loweredExe = Eseq.expr.accept<IRExprLowered>(this);

        IRExprLowered loweredESeq = new IRExprLowered(
            loweredEval.Concat(loweredExe.stmts).ToList<LIRStmt>(),
            loweredExe.expr
        );
        
        return matchThenReturn<T, IRExprLowered>(loweredESeq);
    }

    public T visit<T>(IRCJump cJump) { 
        IRExprLowered loweredCond = cJump.cond.accept<IRExprLowered>(this);
        List<LIRStmt> loweredStmts = loweredCond.stmts;

        loweredStmts.Add(
            new LIRCJump(
                loweredCond.expr,
                cJump.trueLabel,
                cJump.falseLabel
            )
        );
        
        return matchThenReturn<T, List<LIRStmt>>(loweredStmts);
    }

    public T visit<T>(IRExp exp) { 
        IRExprLowered loweredExp = exp.expr.accept<IRExprLowered>(this);
        return matchThenReturn<T, IRExprLowered>(loweredExp);
    }

    public T visit<T>(IRJump jump) { 
        IRExprLowered loweredTarget = jump.target.accept<IRExprLowered>(this);
        
        List<LIRStmt> loweredStmts = loweredTarget.stmts;
        loweredStmts.Add(
            new LIRJump(loweredTarget.expr)
        );

        return matchThenReturn<T, List<LIRStmt>>(loweredStmts);
    }

    public T visit<T>(IRLabel label) { 
        List<LIRStmt> loweredStmts = new List<LIRStmt>();
        loweredStmts.Add(
            new LIRLabel(label.name)
        );

        return matchThenReturn<T, List<LIRStmt>>(loweredStmts);
    }

    public T visit<T>(IRReturn Return) { 
        List<LIRStmt> loweredStmts = new List<LIRStmt>();
        List<LIRExpr> loweredReturnTemps = new List<LIRExpr>();
        foreach(IRExpr irReturn in Return.returns) {
            IRExprLowered loweredReturn = irReturn.accept<IRExprLowered>(this);
            loweredStmts = loweredStmts.Concat(loweredReturn.stmts).ToList();
            LIRTemp newTemp = createNewTemp();

            LIRMoveTemp moveReturnToNewTemp = new LIRMoveTemp(
                loweredReturn.expr,
                newTemp
            );
            loweredStmts.Add(moveReturnToNewTemp);
            loweredReturnTemps.Add(newTemp);
        }

        LIRReturn loweredReturnStmt = new LIRReturn(loweredReturnTemps);
        loweredStmts.Add(loweredReturnStmt);
        
        return matchThenReturn<T, List<LIRStmt>>(loweredStmts);
    }

    public T visit<T>(IRCallStmt callStmt) { 
        List<LIRStmt> allLoweredStmts = new List<LIRStmt>();
        List<LIRExpr> loweredArgTemps = new List<LIRExpr>();
        foreach(IRExpr irArg in callStmt.args) {
            IRExprLowered loweredArg = irArg.accept<IRExprLowered>(this);
            allLoweredStmts = allLoweredStmts.Concat(loweredArg.stmts).ToList();

            LIRTemp newTemp = createNewTemp();
            LIRMoveTemp moveArgIntoNewTemp = new LIRMoveTemp(
                loweredArg.expr,
                newTemp
            );
            allLoweredStmts.Add(moveArgIntoNewTemp);
            loweredArgTemps.Add(newTemp);
        }
        IRExprLowered loweredTarget = callStmt.target.accept<IRExprLowered>(this);
        allLoweredStmts = allLoweredStmts.Concat(loweredTarget.stmts).ToList();

        LIRTemp newTargetTemp = createNewTemp();
        LIRMoveTemp moveTargetToNewTemp = new LIRMoveTemp(
            loweredTarget.expr,
            newTargetTemp
        );
        allLoweredStmts.Add(moveTargetToNewTemp);
        LIRCallM loweredCall = new LIRCallM(
            newTargetTemp, loweredArgTemps, 1
        );
        allLoweredStmts.Add(loweredCall);

        return matchThenReturn<T, List<LIRStmt>>(allLoweredStmts);
    }

    public T visit<T>(IRCall call) { 
        List<LIRStmt> allLoweredStmts = new List<LIRStmt>();
        List<LIRExpr> loweredArgTemps = new List<LIRExpr>();
        foreach(IRExpr irArg in call.args) {
            IRExprLowered loweredArg = irArg.accept<IRExprLowered>(this);
            allLoweredStmts = allLoweredStmts.Concat(loweredArg.stmts).ToList();

            LIRTemp newTemp = createNewTemp();
            LIRMoveTemp moveArgIntoNewTemp = new LIRMoveTemp(
                loweredArg.expr,
                newTemp
            );
            allLoweredStmts.Add(moveArgIntoNewTemp);
            loweredArgTemps.Add(newTemp);
        }
        IRExprLowered loweredTarget = call.target.accept<IRExprLowered>(this);
        allLoweredStmts = allLoweredStmts.Concat(loweredTarget.stmts).ToList();

        LIRTemp newTargetTemp = createNewTemp();
        LIRMoveTemp moveTargetToNewTemp = new LIRMoveTemp(
            loweredTarget.expr,
            newTargetTemp
        );
        allLoweredStmts.Add(moveTargetToNewTemp);
        LIRCallM loweredCall = new LIRCallM(
            newTargetTemp, loweredArgTemps, 1
        );
        allLoweredStmts.Add(loweredCall);

        LIRTemp newFunctionDestTemp = createNewTemp();
        LIRMoveTemp moveReturnValIntoFunctionDest = new LIRMoveTemp(
            new LIRTemp(IRConfiguration.ABSTRACT_RET_PREFIX + 1),
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

        LBinOpType opType;
        switch(binOp.opType) {
            case BinOpType.ADD: opType = LBinOpType.ADD; break; 
            case BinOpType.SUB: opType = LBinOpType.SUB; break; 
            case BinOpType.MUL: opType = LBinOpType.MUL; break; 
            case BinOpType.DIV: opType = LBinOpType.DIV; break; 
            case BinOpType.MOD: opType = LBinOpType.MOD; break; 
            case BinOpType.AND: opType = LBinOpType.AND; break; 
            case BinOpType.OR: opType = LBinOpType.OR; break; 
            case BinOpType.XOR: opType = LBinOpType.XOR; break; 
            case BinOpType.LSHIFT: opType = LBinOpType.LSHIFT; break; 
            case BinOpType.RSHIFT: opType = LBinOpType.RSHIFT; break; 
            case BinOpType.EQ: opType = LBinOpType.EQ; break; 
            case BinOpType.NEQ: opType = LBinOpType.NEQ; break; 
            case BinOpType.LT: opType = LBinOpType.LT; break; 
            case BinOpType.GT: opType = LBinOpType.GT; break; 
            case BinOpType.LEQ: opType = LBinOpType.LEQ; break; 
            case BinOpType.GEQ: opType = LBinOpType.GEQ; break; 
            case BinOpType.ULT: opType = LBinOpType.ULT; break; 
            default:
                throw new Exception("Unable to map IR the binary operation to lowered a IR binary operation");
        }

        if (commutes(loweredRight.stmts, loweredLeft.expr)) {
            List<LIRStmt> allLoweredStmts = loweredLeft.stmts;
            allLoweredStmts = allLoweredStmts.Concat(loweredRight.stmts).ToList();

            LIRBinOp loweredBinOp = new LIRBinOp(
                opType,
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
            List<LIRStmt> allLoweredStmts = loweredLeft.stmts;
            LIRTemp freshTemp = createNewTemp();
            allLoweredStmts.Add(
                new LIRMoveTemp(
                    loweredLeft.expr,
                    freshTemp
                )
            );
            allLoweredStmts = allLoweredStmts.Concat(loweredRight.stmts).ToList();

            LIRBinOp loweredBinOp = new LIRBinOp(
                opType,
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
        LUnaryOpType opType;
        switch(unaryOp.opType) {
            case UnaryOpType.NOT: opType = LUnaryOpType.NOT; break;
            case UnaryOpType.NEGATE: opType = LUnaryOpType.NEGATE; break;
            default:
                throw new Exception("Could not lower the unary operation type for some reason.");
        }

        return matchThenReturn<T, IRExprLowered>(new IRExprLowered(
            loweredOperand.stmts,
            new LIRUnaryOp(
                opType,
                loweredOperand.expr
            )
        ));
    }

    public T visit<T>(IRConst Const) { 
        IRExprLowered loweredConst = new IRExprLowered(
            new List<LIRStmt>(),
            new LIRConst(Const.value)
        );
        
        return matchThenReturn<T, IRExprLowered>(loweredConst);
    }

    public T visit<T>(IRMem mem) { 
        IRExprLowered loweredAddress = mem.expr.accept<IRExprLowered>(this);
        IRExprLowered loweredMem = new IRExprLowered(
            loweredAddress.stmts,
            new LIRMem(loweredAddress.expr)
        );
        
        return matchThenReturn<T, IRExprLowered>(loweredMem);
    }

    public T visit<T>(IRName name) { 
        IRExprLowered loweredExpr = new IRExprLowered(
            new List<LIRStmt>(),
            new LIRName(name.name)
        ); 

        return matchThenReturn<T, IRExprLowered>(loweredExpr);
    }

    public T visit<T>(IRTemp temp) { 
        IRExprLowered loweredTemp = new IRExprLowered(
            new List<LIRStmt>(),
            new LIRTemp(temp.name)
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

    private LIRTemp createNewTemp() {
        LIRTemp temp = new LIRTemp(
            String.Format("t{0}", tempCounter)
        );

        tempCounter++;
        return temp;
    }

    /*
     * Testing if 'loweredStmts' can alter the content of 'loweredExpr' in anyway.
     */
    private bool commutes(List<LIRStmt> loweredStmts, LIRExpr loweredExpr) {
        if (loweredStmts.Count == 0 || isAnUnaffectedLoweredExpr(loweredExpr)) {
            return true;
        }

        Tuple<List<LIRMem>, Dictionary<string, LIRTemp>> usedMemsAndTemps = extractPossibleNonCommutes(loweredStmts);
        List<LIRMem> usedMems = usedMemsAndTemps.Item1;
        Dictionary<string, LIRTemp> usedTemps = usedMemsAndTemps.Item2;

        return hasNoCommutingConflicts(usedMems, usedTemps, loweredExpr);
    }

    private bool isAnUnaffectedLoweredExpr(LIRExpr loweredExpr) {
        switch(loweredExpr) {
            case LIRName loweredName:
            case LIRConst loweredConst:
                return true;
            default:
                return false;
        }
    }

    /*
     * Extracting destinations from the lowered moves that might cause commuting problems.
     */
    private Tuple<List<LIRMem>, Dictionary<string, LIRTemp>> extractPossibleNonCommutes(List<LIRStmt> loweredStmts) {
        List<LIRMem> memsUsed = new List<LIRMem>();
        Dictionary<string, LIRTemp> tempsUsed = new Dictionary<string, LIRTemp>();

        foreach(LIRStmt loweredStmt in loweredStmts) {
            switch(loweredStmt) {
                case LIRMoveTemp moveTemp:
                    tempsUsed.Add(moveTemp.dest.name, moveTemp.dest);
                    break;
                case LIRMoveMem moveMem:
                    memsUsed.Add(moveMem.dest);
                    break;
            }
        }

        return Tuple.Create<List<LIRMem>, Dictionary<string, LIRTemp>>(
            memsUsed,
            tempsUsed
        );
    }

    private bool hasNoCommutingConflicts(
        List<LIRMem> usedMems, Dictionary<string, LIRTemp> usedTemps, LIRExpr expr
    ) {
        switch(expr) {
            case LIRMem loweredMem: throw new Exception("Case for handling memory isn't implemented yet.");
            case LIRTemp loweredTemp: 
                return !usedTemps.ContainsKey(loweredTemp.name);
            case LIRBinOp loweredBinOp:
                return (
                    hasNoCommutingConflicts(usedMems, usedTemps, loweredBinOp.left) 
                    && 
                    hasNoCommutingConflicts(usedMems, usedTemps, loweredBinOp.right)
                );
            case LIRUnaryOp loweredUnaryOp:
                return hasNoCommutingConflicts(usedMems, usedTemps, loweredUnaryOp.operand);
            case LIRName loweredName:
            case LIRConst loweredConst:
                return true;
            default:
                throw new Exception("Something went wrong");
        }
    }

    private struct IRExprLowered {
        public List<LIRStmt> stmts;
        public LIRExpr expr;

        public IRExprLowered(List<LIRStmt> stmts, LIRExpr expr) {
            this.stmts = stmts;
            this.expr = expr;
        }
    }
}