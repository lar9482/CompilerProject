using System.Reflection.Emit;
using CompilerProj.IR;
using CompilerProj.Visitors;

public sealed class LabelAddressBuilder : IRVisitorVoid {
    public Dictionary<string, int> nameToAddress;
    public Dictionary<int, IRNode> addressToInsn;

    private int address;

    public LabelAddressBuilder() {
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

        visitStmt(funcDecl.body);
    }

    private void visitStmt(IRStmt stmt) {
        switch(stmt) {
            case IRSeq seq: seq.accept(this); break;
            case IR_Eseq eSeq: eSeq.accept(this); break;
            case IRLabel label: label.accept(this); break;
            default:
                break;
        }
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

    public void visit(IRSeq seq) { 
        foreach(IRStmt seqStmt in seq.statements) {
            visitStmt(seqStmt);
        }
    }

    public void visit(IR_Eseq Eseq) { 
        visitStmt(Eseq.stmt);
    }

    public void visit(IRLabel label) { 
        addNameToCurrentAddress(label.name);
    }

    public void visit(IRMove move) { throw new NotImplementedException("This visit is not used"); }
    public void visit(IRCJump cJump) { throw new NotImplementedException("This visit is not used"); }
    public void visit(IRExp exp) { throw new NotImplementedException("This visit is not used"); }
    public void visit(IRJump jump) { throw new NotImplementedException("This visit is not used"); }
    public void visit(IRReturn Return) { throw new NotImplementedException("This visit is not used"); }
    public void visit(IRCallStmt callStmt) {throw new NotImplementedException("This visit is not used");  }
    public void visit(IRBinOp binOp) { throw new NotImplementedException("This visit is not used"); }
    public void visit(IRUnaryOp unaryOp) { throw new NotImplementedException("This visit is not used"); }
    public void visit(IRCall call) { throw new NotImplementedException("This visit is not used"); }
    public void visit(IRConst Const) { throw new NotImplementedException("This visit is not used"); }
    public void visit(IRMem mem) { throw new NotImplementedException("This visit is not used"); }
    public void visit(IRName name) { throw new NotImplementedException("This visit is not used"); }
    public void visit(IRTemp temp) { throw new NotImplementedException("This visit is not used"); }
}