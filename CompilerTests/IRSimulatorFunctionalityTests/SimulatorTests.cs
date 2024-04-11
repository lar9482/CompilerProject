using CompilerProj;

namespace CompilerTests;

public class FunctionalityTests {
    [SetUp]
    public void Setup() { }
    
    [Test]
    public void MultiReturnTest() {
        IRCompUnit compUnit = setUp_MultiReturnTest();
        int[] args = new int[] {
            2, 1
        };


        IRSimulator simulator = new IRSimulator(compUnit);
        simulator.call("b", args);
    }

    private IRCompUnit setUp_MultiReturnTest() {
        // IR roughly corresponds to the following:
        //     a(i:int, j:int): int, int {
        //         return i, (2 * j);
        //     }
        //     b(i:int, j:int): int {
        //         x:int, y:int = a(i, j);
        //         return x + 5 * y;
        //     }

        // b(2, 1) {
        //   x, y = 2, 2
        //   return 2 + 5 * 2 // 12;
        //
        string arg1 = IRConfiguration.ABSTRACT_ARG_PREFIX + 1;
        string arg2 = IRConfiguration.ABSTRACT_ARG_PREFIX + 2;

        string ret1 = IRConfiguration.ABSTRACT_RET_PREFIX + 1;
        string ret2 = IRConfiguration.ABSTRACT_RET_PREFIX + 2;

        IRSeq aBody =
                new IRSeq(
                    new List<IRStmt>() {
                        new IRMove(new IRTemp("i"), new IRTemp(arg1)),
                        new IRMove(new IRTemp("j"), new IRTemp(arg2)),
                        new IRReturn(
                            new List<IRExpr>() {
                                new IRTemp("i"),
                                new IRBinOp(BinOpType.MUL, new IRConst(2), new IRTemp("j"))
                            })
                    });
        IRFuncDecl aFunc = new IRFuncDecl("a", aBody);

        IRStmt bBody =
                new IRSeq(
                    new List<IRStmt>() {
                        new IRCallStmt(
                            new IRName("a"), 
                            new List<IRExpr>() {
                                new IRTemp(arg1), new IRTemp(arg2)
                            }, 
                            2
                        ),
                        new IRMove(new IRTemp("x"), new IRTemp(ret1)),
                        new IRMove(new IRTemp("y"), new IRTemp(ret2)),
                        new IRReturn(
                            new List<IRExpr>() {
                                new IRBinOp(
                                        BinOpType.ADD,
                                        new IRTemp("x"),
                                        new IRBinOp(BinOpType.MUL, new IRConst(5), new IRTemp("y"))) 
                        })
                    }
                );
        IRFuncDecl bFunc = new IRFuncDecl("b", bBody);

        Dictionary<string, IRFuncDecl> functions = new Dictionary<string, IRFuncDecl>();
        functions.Add(aFunc.name, aFunc);
        functions.Add(bFunc.name, bFunc);

        IRCompUnit compUnit = new IRCompUnit("test", functions, new List<string>() { });

        return compUnit;
    }
}