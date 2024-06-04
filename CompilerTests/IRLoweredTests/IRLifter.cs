public sealed class IRLifter : LIRVisitorGeneric {

    public T visit<T>(LIRCompUnit compUnit) {
        Dictionary<string, IRFuncDecl> highFuncDecls = new Dictionary<string, IRFuncDecl>();
        foreach(KeyValuePair<string, LIRFuncDecl> lowFuncDeclPair in compUnit.functions) {
            string lowFuncDeclName = lowFuncDeclPair.Key;
            LIRFuncDecl lowFuncDecl = lowFuncDeclPair.Value;

            IRFuncDecl highFuncDecl = lowFuncDecl.accept<IRFuncDecl>(this);
            highFuncDecls.Add(lowFuncDeclName, highFuncDecl);
        }

        return matchThenReturn<T, IRCompUnit>(new IRCompUnit(
            compUnit.name,
            highFuncDecls,
            new List<string>() { }
        ));
    }

    public T visit<T>(LIRFuncDecl funcDecl) {
        List<IRStmt> highStmts = new List<IRStmt>();
        foreach(LIRStmt lowStmt in funcDecl.body) {
            IRStmt highStmt = lowStmt.accept<IRStmt>(this);
            highStmts.Add(highStmt);
        }

        return matchThenReturn<T, IRFuncDecl>(new IRFuncDecl(
            funcDecl.name,
            new IRSeq(highStmts)
        ));
    }

    public T visit<T>(LIRMoveTemp moveTemp) {
        IRExpr highSrc = moveTemp.source.accept<IRExpr>(this);
        IRTemp highTarget = moveTemp.dest.accept<IRTemp>(this);
        
        return matchThenReturn<T, IRMove>(new IRMove(
            highTarget,
            highSrc
        ));
    }

    public T visit<T>(LIRMoveMem moveMem) {
        IRExpr highSrc = moveMem.source.accept<IRExpr>(this);
        IRMem highTarget = moveMem.dest.accept<IRMem>(this);
        
        return matchThenReturn<T, IRMove>(new IRMove(
            highTarget,
            highSrc
        ));
    }

    public T visit<T>(LIRJump jump) {
        IRExpr highTarget = jump.target.accept<IRExpr>(this);

        return matchThenReturn<T, IRJump>(new IRJump(
            highTarget
        ));
    }

    public T visit<T>(LIRCJump cJump) {
        IRExpr highCond = cJump.cond.accept<IRExpr>(this);
        return matchThenReturn<T, IRCJump>(new IRCJump(
            highCond,
            cJump.trueLabel,
            cJump.falseLabel
        ));
    }

    public T visit<T>(LIRReturn Return) {
        List<IRExpr> highReturns = new List<IRExpr>();
        foreach(LIRExpr lowReturn in Return.returns) {
            IRExpr highReturn = lowReturn.accept<IRExpr>(this);
            highReturns.Add(highReturn);
        }

        return matchThenReturn<T, IRReturn>(new IRReturn(
            highReturns
        ));
    }

    public T visit<T>(LIRLabel label) {
        return matchThenReturn<T, IRLabel>(new IRLabel(
            label.label
        ));
    }

    public T visit<T>(LIRCallM callM) {
        IRExpr highTarget = callM.target.accept<IRExpr>(this);
        List<IRExpr> highArgs = new List<IRExpr>();
        foreach(LIRExpr lowArg in callM.args) {
            IRExpr highArg = lowArg.accept<IRExpr>(this);
            highArgs.Add(highArg);
        } 

        return matchThenReturn<T, IRCallStmt>(new IRCallStmt(
            highTarget,
            highArgs,
            callM.nReturns
        ));
    }

    public T visit<T>(LIRBinOp binOp) {
        BinOpType opType;
        switch(binOp.opType) {
            case LBinOpType.ADD: opType = BinOpType.ADD; break;
            case LBinOpType.SUB: opType = BinOpType.SUB; break;
            case LBinOpType.MUL: opType = BinOpType.MUL; break;
            case LBinOpType.DIV: opType = BinOpType.DIV; break;
            case LBinOpType.MOD: opType = BinOpType.MOD; break;
            case LBinOpType.AND: opType = BinOpType.AND; break;
            case LBinOpType.OR: opType = BinOpType.OR; break;
            case LBinOpType.XOR: opType = BinOpType.XOR; break;
            case LBinOpType.LSHIFT: opType = BinOpType.LSHIFT; break;
            case LBinOpType.RSHIFT: opType = BinOpType.RSHIFT; break;
            case LBinOpType.EQ: opType = BinOpType.EQ; break;
            case LBinOpType.NEQ: opType = BinOpType.NEQ; break;
            case LBinOpType.LT: opType = BinOpType.LT; break;
            case LBinOpType.GT: opType = BinOpType.GT; break;
            case LBinOpType.LEQ: opType = BinOpType.LEQ; break;
            case LBinOpType.GEQ: opType = BinOpType.GEQ; break;
            case LBinOpType.ULT: opType = BinOpType.ULT; break;
            default:
                throw new Exception("Unable to match the binary operation");
        }

        IRExpr highLeftOperand = binOp.left.accept<IRExpr>(this);
        IRExpr highRightOperand = binOp.right.accept<IRExpr>(this);
        
        return matchThenReturn<T, IRBinOp>(new IRBinOp(
            opType,
            highLeftOperand,
            highRightOperand
        ));
    }

    public T visit<T>(LIRUnaryOp unaryOp) {
        UnaryOpType opType;
        switch(unaryOp.opType) {
            case LUnaryOpType.NOT: opType = UnaryOpType.NOT; break;
            case LUnaryOpType.NEGATE: opType = UnaryOpType.NEGATE; break;
            default:
                throw new Exception("Unable to match the unary operation");
        }

        IRExpr highOperand = unaryOp.operand.accept<IRExpr>(this);
        return matchThenReturn<T, IRUnaryOp>(new IRUnaryOp(
            opType,
            highOperand
        ));
    }

    public T visit<T>(LIRMem mem) {
        IRExpr highAddress = mem.address.accept<IRExpr>(this);

        return matchThenReturn<T, IRMem>(new IRMem(
            MemType.NORMAL,
            highAddress
        ));
    }

    public T visit<T>(LIRTemp temp) {
        return matchThenReturn<T, IRTemp>(new IRTemp(
            temp.name
        ));
    }

    public T visit<T>(LIRName name) {
        return matchThenReturn<T, IRName>(new IRName(
            name.name
        ));
    }

    public T visit<T>(LIRConst Const) {
        return matchThenReturn<T, IRConst>(new IRConst(
            Const.value
        ));
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
}