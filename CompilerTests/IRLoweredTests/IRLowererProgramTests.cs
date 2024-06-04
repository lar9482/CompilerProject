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

        int retVal2 = sim2.call(functionCall, args);
        int retVal1 = sim1.call(functionCall, args);

        Assert.That(retVal1, Is.EqualTo(retVal2));
    }

    private void ensureConsoleOutputIsEqual(string filePath, string functionCall, int[] args, string pattern) {
        Tuple<IRCompUnit, IRCompUnit> bothIR = getBothVersionsOfIR(filePath);
        IRCompUnit originalIR = bothIR.Item1;
        IRCompUnit liftedIR = bothIR.Item2;

        IRSimulator sim1 = new IRSimulator(originalIR);
        IRSimulator sim2 = new IRSimulator(liftedIR);

        sim1.call(functionCall, args);
        sim2.call(functionCall, args);

        string consoleOutput = sim2.consoleOutputCapture.ToString();
        Assert.That(consoleOutput.Length, Is.EqualTo(pattern.Length * 2));
        string firstConsoleOutput = consoleOutput.Substring(0, pattern.Length);
        string secondConsoleOutput = consoleOutput.Substring(pattern.Length);
        Assert.That(firstConsoleOutput, Is.EqualTo(pattern));
        Assert.That(secondConsoleOutput, Is.EqualTo(pattern));
    }

    [Test]
    public void funcCall_multiReturn() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/funcCall_multiReturn.prgm";
        int[] args = new int[] {
            2, 1
        };

        ensureRetValsAreEqual(filePath, "b", args);
    }

    [Test]
    public void funcCall_singleReturn1() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/funcCall_singleReturn1.prgm";
        int[] args = new int[] { 10 };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void funcCall_singleReturn2() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/funcCall_singleReturn2.prgm";
        int[] args = new int[] { };

        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void funcCall_singleReturn3() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/funcCall_singleReturn3.prgm";
        int[] args = new int[] { };

        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void exprInteger() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/exprInteger.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void exprBool() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/exprBool.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void arrayDecl_noInitialValues() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/arrayDecl_noInitialValues.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void arrayDecl_noInitialValues_thenAssigned() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/arrayDecl_noInitialValues_thenAssigned.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void arrayDecl_hasInitialValues() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/arrayDecl_hasInitialValues.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }
    //TODO:Handle this later
    [Test]
    public void arrayDecl_outOfBounds_Positive() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/arrayDecl_outOfBounds_Positive.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        try {
            int[] args = new int[] { };
            IRSimulator simulator = new IRSimulator(IR);
            simulator.call("main", args);
            Assert.Fail();
        } catch(Exception e) {
            string errorMsg = e.Message;
            bool isOutOfBoundsError = errorMsg.Contains("Out of bounds!");
            Assert.That(isOutOfBoundsError, Is.True);
        }
    }

    //TODO:Handle this later
    [Test]
    public void arrayDecl_outOfBounds_Negative() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/arrayDecl_outOfBounds_Negative.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        try {
            int[] args = new int[] { };
            IRSimulator simulator = new IRSimulator(IR);
            simulator.call("main", args);
            Assert.Fail();
        } catch(Exception e) {
            string errorMsg = e.Message;
            bool isOutOfBoundsError = errorMsg.Contains("Out of bounds!");
            Assert.That(isOutOfBoundsError, Is.True);
        }
    }

    [Test]
    public void arrayDeclCall() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/arrayDeclCall.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void whileLoop_10Iterations() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/whileLoop_10Iterations.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void whileLoop_0Iterations() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/whileLoop_0Iterations.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    //TODO: Handle this later.
    [Test]
    public void forLoopSimple() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/forLoopSimple.prgm";
        int[] args = new int[] { };
        ensureConsoleOutputIsEqual(filePath, "main", args, "10");
    }

    [Test]
    public void forLoop_DeclsInit() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/forLoop_DeclsInit.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void forLoop_NonDeclsInit() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/forLoop_NonDeclsInit.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }
}