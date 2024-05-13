using CompilerProj.Visitors;

public sealed class IRLowerer : IRVisitorGeneric {
    private int tempCounter;

    public IRLowerer(int tempCounter) {
        this.tempCounter = tempCounter;
    }

    public T visit<T>(IRCompUnit compUnit) { throw new NotImplementedException(); }
    public T visit<T>(IRFuncDecl funcDecl) { throw new NotImplementedException(); }

    public T visit<T>(IRMove move) { 
        switch(move.target) {
            case IRMem mem:
            case IRTemp temp:
            default:
                break;
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
    public T visit<T>(IRCall call) { throw new NotImplementedException(); }
    
    //Expr IR
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

    private IRTemp createNewTemp() {
        IRTemp temp = new IRTemp(
            String.Format("t{0}", tempCounter)
        );

        tempCounter++;
        return temp;
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