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
                break;
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
        throw new NotImplementedException(); 
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

    public T visit<T>(IRReturn Return) { throw new NotImplementedException(); }
    public T visit<T>(IRCallStmt callStmt) { throw new NotImplementedException(); }

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
    public T visit<T>(IRBinOp binOp) { throw new NotImplementedException(); }
    public T visit<T>(IRUnaryOp unaryOp) { throw new NotImplementedException(); }

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

    private bool commutes() {
        return true;
    }

    private sealed class IRExprLowered {
        public List<LIRStmt> stmts;
        public LIRExpr expr;

        public IRExprLowered(List<LIRStmt> stmts, LIRExpr expr) {
            this.stmts = stmts;
            this.expr = expr;
        }
    }
}