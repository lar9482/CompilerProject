using CompilerProj;

namespace CompilerTests;

public class IRLowerHardCodedTests {
    
    [Test]
    public void test1() {
        IRTemp x = new IRTemp("x");
        IRTemp y = new IRTemp("y");
        IRMove example1 = new IRMove(
            new IRMem(
                MemType.NORMAL,
                x
            ),
            new IR_Eseq(
                new IRMove(x, y),
                new IRBinOp(
                    BinOpType.ADD,
                    x,
                    new IRConst(1)
                )
            )
        );

        IRLowerer lowerer = new IRLowerer(0);
        List<LIRStmt> stmts = lowerer.visit<List<LIRStmt>>(example1);

        Assert.That(stmts.Count, Is.EqualTo(3));
        Assert.That(stmts[0].GetType(), Is.EqualTo(typeof(LIRMoveTemp)));
        Assert.That(stmts[1].GetType(), Is.EqualTo(typeof(LIRMoveTemp)));
        Assert.That(stmts[2].GetType(), Is.EqualTo(typeof(LIRMoveMem)));

        LIRMoveTemp firstMove = (LIRMoveTemp) stmts[0];
        Assert.That(firstMove.dest.name, Is.EqualTo("t0"));
        Assert.That(firstMove.source.GetType(), Is.EqualTo(typeof(LIRTemp)));
        Assert.That(((LIRTemp) firstMove.source).name, Is.EqualTo("x"));

        LIRMoveTemp secondMove = (LIRMoveTemp) stmts[1];
        Assert.That(secondMove.dest.name, Is.EqualTo("x"));
        Assert.That(secondMove.source.GetType(), Is.EqualTo(typeof(LIRTemp)));
        Assert.That(((LIRTemp) secondMove.source).name, Is.EqualTo("y"));

        LIRMoveMem thirdMove = (LIRMoveMem) stmts[2];
        Assert.That(thirdMove.dest.GetType(), Is.EqualTo(typeof(LIRMem)));
        Assert.That(thirdMove.source.GetType(), Is.EqualTo(typeof(LIRBinOp)));
        LIRMem dest = (LIRMem) thirdMove.dest;
        LIRBinOp src = (LIRBinOp) thirdMove.source;
        Assert.That(dest.address.GetType(), Is.EqualTo(typeof(LIRTemp)));
        Assert.That(((LIRTemp) dest.address).name, Is.EqualTo("t0"));
        Assert.That(src.left.GetType(), Is.EqualTo(typeof(LIRTemp)));
        Assert.That(src.right.GetType(), Is.EqualTo(typeof(LIRConst)));
        Assert.That(((LIRTemp) src.left).name, Is.EqualTo("x"));
        Assert.That(((LIRConst) src.right).value, Is.EqualTo(1));
        Assert.That(src.opType, Is.EqualTo(LBinOpType.ADD));
    }

    [Test]
    public void test2() {
        IRTemp x = new IRTemp("x");
        IRTemp y = new IRTemp("y");
        IRMove example2 = new IRMove(
            new IRMem(
                MemType.NORMAL,
                y
            ),
            new IR_Eseq(
                new IRMove(x, y),
                new IRBinOp(
                    BinOpType.ADD,
                    x,
                    new IRConst(1)
                )
            )
        );

        IRLowerer lowerer = new IRLowerer(0);
        List<LIRStmt> stmts = lowerer.visit<List<LIRStmt>>(example2);

        Assert.That(stmts.Count, Is.EqualTo(2));
        Assert.That(stmts[0].GetType(), Is.EqualTo(typeof(LIRMoveTemp)));
        Assert.That(stmts[1].GetType(), Is.EqualTo(typeof(LIRMoveMem)));

        LIRMoveTemp firstMove = (LIRMoveTemp) stmts[0];
        Assert.That(firstMove.dest.name, Is.EqualTo("x"));
        Assert.That(firstMove.source.GetType(), Is.EqualTo(typeof(LIRTemp)));
        Assert.That(((LIRTemp) firstMove.source).name, Is.EqualTo("y"));

        LIRMoveMem thirdMove = (LIRMoveMem) stmts[1];
        Assert.That(thirdMove.dest.GetType(), Is.EqualTo(typeof(LIRMem)));
        Assert.That(thirdMove.source.GetType(), Is.EqualTo(typeof(LIRBinOp)));
        LIRMem dest = (LIRMem) thirdMove.dest;
        LIRBinOp src = (LIRBinOp) thirdMove.source;
        Assert.That(dest.address.GetType(), Is.EqualTo(typeof(LIRTemp)));
        Assert.That(((LIRTemp) dest.address).name, Is.EqualTo("y"));
        Assert.That(src.left.GetType(), Is.EqualTo(typeof(LIRTemp)));
        Assert.That(src.right.GetType(), Is.EqualTo(typeof(LIRConst)));
        Assert.That(((LIRTemp) src.left).name, Is.EqualTo("x"));
        Assert.That(((LIRConst) src.right).value, Is.EqualTo(1));
        Assert.That(src.opType, Is.EqualTo(LBinOpType.ADD));
    }
}