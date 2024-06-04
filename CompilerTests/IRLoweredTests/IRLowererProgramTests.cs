using CompilerProj;
using NUnit.Framework.Constraints;

namespace CompilerTests;

public class IRLowererProgramTests {
    [SetUp]
    public void Setup() {}

    private Tuple<IRCompUnit, IRCompUnit> getBothVersionsOfIR(string filePath) {
        IRCompUnit IR = Compiler.generateIR(filePath);
        LIRCompUnit loweredIR = Compiler.lowerIR(filePath);

        IRLifter lifter = new IRLifter();
        IRCompUnit liftedIR = lifter.visit<IRCompUnit>(loweredIR);

        return Tuple.Create<IRCompUnit, IRCompUnit>(IR, liftedIR);
    }

    private void ensureRetValsAreEqual(string filePath, string functionCall, int[] args) {
        Tuple<IRCompUnit, IRCompUnit> bothIR = getBothVersionsOfIR(filePath);
        IRCompUnit originalIR = bothIR.Item1;
        IRCompUnit liftedIR = bothIR.Item2;

        IRSimulator sim1 = new IRSimulator(originalIR);
        IRSimulator sim2 = new IRSimulator(liftedIR);

        int retVal1 = sim1.call(functionCall, args);
        int retVal2 = sim2.call(functionCall, args);

        Assert.That(retVal1, Is.EqualTo(retVal2));
    }

    [Test]
    public void funcCall_multiReturn() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/funcCall_multiReturn.prgm";
        int[] args = new int[] {
            2, 1
        };

        ensureRetValsAreEqual(filePath, "b", args);
    }
}