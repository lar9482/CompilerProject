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
    }
}