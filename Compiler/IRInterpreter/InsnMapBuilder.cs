using System.Reflection.Emit;
using CompilerProj.IR;
using CompilerProj.Visitors;

/*
 * Linearizing the IR into a sequence of address to instruction maps.
 * This is done by traversing over the IR in post-order.
 */
public sealed class InsnMapBuilder : IRVisitorVoid {
    public Dictionary<string, int> nameToAddress;
    public Dictionary<int, IRNode> addressToInsn;

    private int address;

    public InsnMapBuilder() {
        this.nameToAddress = new Dictionary<string, int>();
        this.addressToInsn = new Dictionary<int, IRNode>();

        this.address = 0;
    }

    public void visit(IRCompUnit compUnit) { 
        foreach(KeyValuePair<string, IRFuncDecl> funcDeclPair in compUnit.functions) {
            IRFuncDecl funcDecl = funcDeclPair.Value;
            funcDecl.accept(this);
        }

    }

    public void visit(IRFuncDecl funcDecl) { 
        addNameToCurrentAddress(funcDecl.name);
        addInsn(funcDecl);
        funcDecl.body.accept(this);
    }

    public void visit(IRSeq seq) { 
        foreach(IRStmt stmt in seq.statements) {
            stmt.accept(this);
        }
    }

    public void visit(IR_Eseq Eseq) { 
        Eseq.stmt.accept(this);
        Eseq.expr.accept(this);
    }

    public void visit(IRLabel label) { 
        addNameToCurrentAddress(label.name);
    }

    public void visit(IRMove move) { 
        move.src.accept(this);
        move.target.accept(this);
        addInsn(move);
    }

    public void visit(IRCJump cJump) { 
        cJump.cond.accept(this);
        addInsn(cJump);
    }

    public void visit(IRExp exp) { 
        exp.expr.accept(this);
        addInsn(exp);
    }

    public void visit(IRJump jump) { 
        jump.target.accept(this);
        addInsn(jump);
    }

    public void visit(IRReturn Return) { 
        foreach(IRExpr expr in Return.returns) {
            expr.accept(this);
        }

        addInsn(Return);
    }

    public void visit(IRCallStmt callStmt) {
        callStmt.target.accept(this);
        foreach(IRExpr exprArg in callStmt.args) {
            exprArg.accept(this);
        }
        
        addInsn(callStmt);
    }

    public void visit(IRBinOp binOp) { 
        binOp.left.accept(this);
        binOp.right.accept(this);
        addInsn(binOp);
    }

    public void visit(IRUnaryOp unaryOp) { 
        unaryOp.operand.accept(this);
        addInsn(unaryOp);
    }

    public void visit(IRCall call) { 
        call.target.accept(this);
        foreach(IRExpr argExpr in call.args) {
            argExpr.accept(this);
        }

        addInsn(call);
    }

    public void visit(IRConst Const) { 
        addInsn(Const);
    }

    public void visit(IRMem mem) { 
        mem.expr.accept(this);
        addInsn(mem);
    }

    public void visit(IRName name) { 
        addInsn(name);
    }

    public void visit(IRTemp temp) { 
        addInsn(temp);
    }

    private void addInsn(IRNode node) {
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