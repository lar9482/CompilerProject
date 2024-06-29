
/*
 * This visitor will produce a pretty printer of the lowered IR.
 * This will help with debugging.
 */
using System.Collections;

public sealed class PrintVisitor : LIRVisitorVoid {
    public List<string> printContent;
    private string currentLine;

    public PrintVisitor() {
        this.printContent = new List<string>();
        this.currentLine ="";
    }

    public void visit(LIRCompUnit compUnit) {
        foreach(KeyValuePair<string, LIRFuncDecl> funcDeclPair in compUnit.functions) {
            LIRFuncDecl funcDecl = funcDeclPair.Value;
            funcDecl.accept(this);
        }
    }

    public void visit(LIRFuncDecl funcDecl) {
        foreach(LIRStmt irStmt in funcDecl.body) {
            if (irStmt.GetType() != typeof(LIRLabel)) {
                currentLine += "\t";
            }
            irStmt.accept(this);
            printContent.Add(currentLine);
            currentLine = "";
        }
    }

    public void visit(LIRMoveTemp moveTemp) {
        currentLine += "Move(";
        moveTemp.dest.accept(this);
        currentLine += ", ";
        moveTemp.source.accept(this);
        currentLine += ")";
    }

    public void visit(LIRMoveMem moveMem) {
        currentLine += "Move(";
        moveMem.dest.accept(this);
        currentLine += ", ";
        moveMem.source.accept(this);
        currentLine += ")";
    }

    public void visit(LIRJump jump) {
        currentLine += "Jump(";
        jump.target.accept(this);
        currentLine += ")";
    }

    public void visit(LIRCJump cJump) {
        currentLine += "CJump(";
        cJump.cond.accept(this);
        currentLine += String.Format(", {0}, {1})", cJump.trueLabel, cJump.falseLabel);
    }

    public void visit(LIRReturn Return) {
        currentLine += "Return(";
        for (int i = 0; i < Return.returns.Count; i++) {
            LIRExpr returnExpr = Return.returns[i];
            returnExpr.accept(this);

            if (i != (Return.returns.Count - 1)) {
                currentLine += ", ";
            }
        }
        currentLine += ")";
    }

    public void visit(LIRLabel label) {
        currentLine += String.Format("{0}:", label.label);
    }

    public void visit(LIRCallM callM) {
        currentLine += String.Format("Call_{0}(", callM.nReturns);
        callM.target.accept(this);
        if (callM.args.Count > 0) {
            currentLine += ", ";
        }

        for(int i = 0; i < callM.args.Count; i++) {
            LIRExpr arg = callM.args[i];
            arg.accept(this);

            if (i != (callM.args.Count - 1)) {
                currentLine += ", ";
            }
        }
        currentLine += ")";
    }

    public void visit(LIRBinOp binOp) {
        binOp.left.accept(this);
        
        switch(binOp.opType) {
            case LBinOpType.ADD: currentLine += " + "; break;
            case LBinOpType.SUB: currentLine += " - "; break;
            case LBinOpType.MUL: currentLine += " * "; break;
            case LBinOpType.DIV: currentLine += " / "; break;
            case LBinOpType.MOD: currentLine += " % "; break;
            case LBinOpType.AND: currentLine += " && "; break;
            case LBinOpType.OR: currentLine += " || "; break;
            case LBinOpType.XOR: currentLine += " ^ "; break;
            case LBinOpType.LSHIFT:  currentLine += " << "; break;
            case LBinOpType.RSHIFT: currentLine += " >> "; break;
            case LBinOpType.EQ: currentLine += " == "; break;
            case LBinOpType.NEQ: currentLine += " != "; break;
            case LBinOpType.LT: currentLine += " < "; break;
            case LBinOpType.GT: currentLine += " > "; break;
            case LBinOpType.LEQ: currentLine += " <= "; break;
            case LBinOpType.GEQ: currentLine += " >= "; break;
            case LBinOpType.ULT: currentLine += " U< "; break;
        }

        binOp.right.accept(this);
    }

    public void visit(LIRUnaryOp unaryOp) {
        switch (unaryOp.opType) {
            case LUnaryOpType.NOT: currentLine += "!"; break;
            case LUnaryOpType.NEGATE: currentLine += "-"; break;
        }
        unaryOp.operand.accept(this);
    }

    public void visit(LIRMem mem) {
        currentLine += "[";
        mem.address.accept(this);
        currentLine += "]";
    }

    public void visit(LIRTemp temp) {
        currentLine += temp.name;
    }

    public void visit(LIRName name) {
        currentLine += name.name;
    }

    public void visit(LIRConst Const) {
        currentLine += Const.value;
    }
}