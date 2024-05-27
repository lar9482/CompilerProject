
/*
 * Linearizing the lowered IR into a sequence of address to instruction map.
 */
public sealed class LowInsnMapBuilder : LIRVisitorVoid {
    public Dictionary<string, int> nameToAddress;
    public Dictionary<int, LIRNode> addressToInsn;

    private int address;

    public LowInsnMapBuilder() {
        this.nameToAddress = new Dictionary<string, int>();
        this.addressToInsn = new Dictionary<int, LIRNode>();

        this.address = 0;
    }

    public void visit(LIRCompUnit compUnit) {
        foreach(KeyValuePair<string, LIRFuncDecl> funcDeclPair in compUnit.functions) {
            LIRFuncDecl funcDecl = funcDeclPair.Value;
            funcDecl.accept(this);
        }

        LIRLabel outOfBoundsLabel = new LIRLabel(IRConfiguration.OUT_OF_BOUNDS_FLAG);
        LIRCallM outOfBoundsCall = new LIRCallM(
            new LIRName(IRConfiguration.OUT_OF_BOUNDS_FLAG), 
            new List<LIRExpr>() {},
            0
        );

        outOfBoundsLabel.accept(this);
        outOfBoundsCall.accept(this);
    }

    public void visit(LIRFuncDecl funcDecl) {
        foreach(LIRStmt bodyStmt in funcDecl.body) {
            bodyStmt.accept(this);
        }
    }

    public void visit(LIRMoveTemp moveTemp) {
        moveTemp.dest.accept(this);
        moveTemp.source.accept(this);
        addInsn(moveTemp);
    }

    public void visit(LIRMoveMem moveMem) {
        moveMem.dest.accept(this);
        moveMem.source.accept(this);
        addInsn(moveMem);
    }

    public void visit(LIRJump jump) {
        jump.target.accept(this);
        addInsn(jump);
    }

    public void visit(LIRCJump cJump) {
        cJump.cond.accept(this);
        addInsn(cJump);
    }

    public void visit(LIRReturn Return) {
        foreach(LIRExpr returnArg in Return.returns) {
            returnArg.accept(this);
        }

        addInsn(Return);
    }

    public void visit(LIRLabel label) {
        addNameToCurrentAddress(label.label);
        addInsn(label);
    }

    public void visit(LIRCallM callM) {
        callM.target.accept(this);
        foreach(LIRExpr arg in callM.args) {
            arg.accept(this);
        }

        addInsn(callM);
    }

    public void visit(LIRBinOp binOp) {
        binOp.left.accept(this);
        binOp.right.accept(this);
        addInsn(binOp);
    }

    public void visit(LIRUnaryOp unaryOp) {
        unaryOp.operand.accept(this);
        addInsn(unaryOp);
    }

    public void visit(LIRMem mem) {
        mem.address.accept(this);
        addInsn(mem);
    }

    public void visit(LIRTemp temp) {
        addInsn(temp);
    }

    public void visit(LIRName name) {
        addInsn(name);
    }

    public void visit(LIRConst Const) {
        addInsn(Const);
    }

    private void addInsn(LIRNode node) {
        addressToInsn.Add(address, node);
        address++;
    }

    private void addNameToCurrentAddress(string name) {
        if (nameToAddress.ContainsKey(name))
            throw new Exception(
                String.Format(
                    "IRSimulator Error: {0} is already declared in the IR", name
                )
            );
        nameToAddress.Add(name, address);
    }
}