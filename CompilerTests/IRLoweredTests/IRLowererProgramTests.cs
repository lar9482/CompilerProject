using CompilerProj;

namespace CompilerTests;

public class IRLowererProgramTests {

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

        IRLowererHIR lowerer = new IRLowererHIR(0);
        List<IRStmt> stmts = lowerer.visit<List<IRStmt>>(example1);
    }
}