using CompilerProj.Context;
using CompilerProj.Visitors;

public sealed class IRGenerator : ASTVisitorGeneric {
    private Context context;

    public IRGenerator() {
        this.context = new Context();
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
        DummyClass test = new DummyClass();
        
        program.scope = context.pop();
        return matchThenReturn<T, DummyClass>(test);
    }

    public T visit<T>(VarDeclAST varDecl) { throw new NotImplementedException(); }
    public T visit<T>(MultiVarDeclAST multiVarDecl) { throw new NotImplementedException(); }
    public T visit<T>(MultiVarDeclCallAST multiVarDeclCall) { throw new NotImplementedException(); }
    public T visit<T>(ArrayDeclAST array) { throw new NotImplementedException(); }
    public T visit<T>(MultiDimArrayDeclAST multiDimArray) { throw new NotImplementedException(); }
    public T visit<T>(FunctionAST function) { throw new NotImplementedException(); }
    public T visit<T>(ParameterAST parameter) { throw new NotImplementedException(); }
    public T visit<T>(BlockAST block) { throw new NotImplementedException(); }

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
    
    public T visit<T>(ArrayAssignAST arrayAssign) { throw new NotImplementedException(); }
    public T visit<T>(MultiDimArrayAssignAST multiDimArrayAssign) { throw new NotImplementedException(); }

    //TODO: Implement conditionals and while loops.
    public T visit<T>(ConditionalAST conditional) { throw new NotImplementedException(); }
    public T visit<T>(WhileLoopAST whileLoop) { throw new NotImplementedException(); }

    public T visit<T>(ReturnAST returnStmt) { 
        throw new NotImplementedException(); 
    }

    public T visit<T>(ProcedureCallAST procedureCall) { 
        throw new NotImplementedException(); 
    }
    
    //TODO: Implement binary expressions for booleans with short circuiting.
    public T visit<T>(BinaryExprAST binaryExpr) { 
        switch(binaryExpr.exprType) {
            case BinaryExprType.ADD:
            case BinaryExprType.SUB:
            case BinaryExprType.MULT:
            case BinaryExprType.DIV:
            case BinaryExprType.MOD:
                return matchThenReturn<T, IRBinOp>(
                    processIntegerBinExpr(binaryExpr)
                );
            default:
                throw new Exception(
                    String.Format(
                        "IRGenerator: {0} is not a supported binary operator", binaryExpr.exprType
                    )
                );
        }
    }

    private IRBinOp processIntegerBinExpr(BinaryExprAST binaryExpr) {
        IRExpr irLeft = binaryExpr.leftOperand.accept<IRExpr>(this);
        IRExpr irRight = binaryExpr.rightOperand.accept<IRExpr>(this);
        BinOpType opType;

        switch(binaryExpr.exprType) {
            case BinaryExprType.ADD: opType = BinOpType.ADD; break;
            case BinaryExprType.SUB: opType = BinOpType.SUB; break;
            case BinaryExprType.MULT: opType = BinOpType.MUL; break;
            case BinaryExprType.DIV: opType = BinOpType.DIV; break;
            case BinaryExprType.MOD: opType = BinOpType.MOD; break;
            default:
                throw new Exception(
                    String.Format(
                        "IRGenerator: {0} is not supported as a binary operation inbetween two integers.",
                        binaryExpr.exprType
                    )
                );
        }

        return new IRBinOp(
            opType, irLeft, irRight
        );
    }

    //TODO: Implement unary expressions for booleans with short circuiting.
    public T visit<T>(UnaryExprAST unaryExpr) { 
        switch(unaryExpr.exprType) {
            case UnaryExprType.NEGATE:
                return matchThenReturn<T, IRUnaryOp>(
                    processIntegerUnaryExpr(unaryExpr)
                );
            default:
                throw new Exception(
                    String.Format(
                        "IRGenerator: {0} is not supported as a unary operation.",
                        unaryExpr.exprType
                    )
                );
        }
    }

    private IRUnaryOp processIntegerUnaryExpr(UnaryExprAST unaryExpr) {
        IRExpr irOperand = unaryExpr.operand.accept<IRExpr>(this);

        return new IRUnaryOp(
            UnaryOpType.NEGATE, irOperand
        );
    }
    
    public T visit<T>(VarAccessAST varAccess) { 
        IRTemp temp = new IRTemp(varAccess.variableName);
        return matchThenReturn<T, IRTemp>(temp);
    }

    public T visit<T>(ArrayAccessAST arrayAccess) { throw new NotImplementedException(); }
    public T visit<T>(MultiDimArrayAccessAST multiDimArrayAccess) { throw new NotImplementedException(); }

    /*
     * This implementation should only be invoked from an expression.
     */
    public T visit<T>(FunctionCallAST functionCall) { 
        throw new NotImplementedException();
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
}