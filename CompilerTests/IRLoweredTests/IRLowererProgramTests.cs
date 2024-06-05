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
        string filePath = "../../../ProgramFiles/funcCall_multiReturn.prgm";
        int[] args = new int[] {
            2, 1
        };

        ensureRetValsAreEqual(filePath, "b", args);
    }

    [Test]
    public void funcCall_singleReturn1() {
        string filePath = "../../../ProgramFiles/funcCall_singleReturn1.prgm";
        int[] args = new int[] { 10 };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void funcCall_singleReturn2() {
        string filePath = "../../../ProgramFiles/funcCall_singleReturn2.prgm";
        int[] args = new int[] { };

        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void funcCall_singleReturn3() {
        string filePath = "../../../ProgramFiles/funcCall_singleReturn3.prgm";
        int[] args = new int[] { };

        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void exprInteger() {
        string filePath = "../../../ProgramFiles/exprInteger.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void exprBool() {
        string filePath = "../../../ProgramFiles/exprBool.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void arrayDecl_noInitialValues() {
        string filePath = "../../../ProgramFiles/arrayDecl_noInitialValues.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void arrayDecl_noInitialValues_thenAssigned() {
        string filePath = "../../../ProgramFiles/arrayDecl_noInitialValues_thenAssigned.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void arrayDecl_hasInitialValues() {
        string filePath = "../../../ProgramFiles/arrayDecl_hasInitialValues.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void arrayDecl_outOfBounds_Positive() {
        string filePath = "../../../ProgramFiles/arrayDecl_outOfBounds_Positive.prgm";
        int[] args = new int[] { };
        Tuple<IRCompUnit, IRCompUnit> bothIR = getBothVersionsOfIR(filePath);
        try {
            IRSimulator sim = new IRSimulator(bothIR.Item1);
            sim.call("main", args);
            Assert.Fail();
        } catch (Exception e) {
            string errorMsg = e.Message;
            bool isOutOfBoundsError = errorMsg.Contains("Out of bounds!");
            Assert.That(isOutOfBoundsError, Is.True);
        }

        try {
            IRSimulator sim = new IRSimulator(bothIR.Item2);
            sim.call("main", args);
            Assert.Fail();
        } catch (Exception e) {
            string errorMsg = e.Message;
            bool isOutOfBoundsError = errorMsg.Contains("Out of bounds!");
            Assert.That(isOutOfBoundsError, Is.True);
        }
    }

    [Test]
    public void arrayDecl_outOfBounds_Negative() {
        string filePath = "../../../ProgramFiles/arrayDecl_outOfBounds_Negative.prgm";
        int[] args = new int[] { };
        Tuple<IRCompUnit, IRCompUnit> bothIR = getBothVersionsOfIR(filePath);
        try {
            IRSimulator sim = new IRSimulator(bothIR.Item1);
            sim.call("main", args);
            Assert.Fail();
        } catch (Exception e) {
            string errorMsg = e.Message;
            bool isOutOfBoundsError = errorMsg.Contains("Out of bounds!");
            Assert.That(isOutOfBoundsError, Is.True);
        }

        try {
            IRSimulator sim = new IRSimulator(bothIR.Item2);
            sim.call("main", args);
            Assert.Fail();
        } catch (Exception e) {
            string errorMsg = e.Message;
            bool isOutOfBoundsError = errorMsg.Contains("Out of bounds!");
            Assert.That(isOutOfBoundsError, Is.True);
        }
    }

    [Test]
    public void arrayDeclCall() {
        string filePath = "../../../ProgramFiles/arrayDeclCall.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void whileLoop_10Iterations() {
        string filePath = "../../../ProgramFiles/whileLoop_10Iterations.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void whileLoop_0Iterations() {
        string filePath = "../../../ProgramFiles/whileLoop_0Iterations.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void forLoopSimple() {
        string filePath = "../../../ProgramFiles/forLoopSimple.prgm";
        int[] args = new int[] { };
        ensureConsoleOutputIsEqual(filePath, "main", args, "10");
    }

    [Test]
    public void forLoop_DeclsInit() {
        string filePath = "../../../ProgramFiles/forLoop_DeclsInit.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void forLoop_NonDeclsInit() {
        string filePath = "../../../ProgramFiles/forLoop_NonDeclsInit.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void ifStmt() {
        string filePath = "../../../ProgramFiles/ifStmt.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void if_elseif_stmts() {
        string filePath = "../../../ProgramFiles/if_elseif_stmts.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void if_else_stmt() {
        string filePath = "../../../ProgramFiles/if_else_stmt.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void if_elseif_else_stmts() {
        string filePath = "../../../ProgramFiles/if_elseif_else_stmts.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void multiDimArrayDecl_noInitVals() {
        string filePath = "../../../ProgramFiles/multiDimArrayDecl_noInitVals.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void multiDimArrayDecl_noInitVals_thenAssign() {
        string filePath = "../../../ProgramFiles/multiDimArrayDecl_noInitVals_thenAssign.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void multiDimArrayDecl_initVals() {
        string filePath = "../../../ProgramFiles/multiDimArrayDecl_initVals.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void multiDimArrayDeclCall() {
        string filePath = "../../../ProgramFiles/multiDimArrayDeclCall.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void printlnHelloWorld() {
        string filePath = "../../../ProgramFiles/printlnHelloWorld.prgm";
        int[] args = new int[] { };
        ensureConsoleOutputIsEqual(filePath, "main", args, "Hello World\r\n");
    }

    [Test]
    public void printHelloWorld() {
        string filePath = "../../../ProgramFiles/printHelloWorld.prgm";
        int[] args = new int[] { };
        ensureConsoleOutputIsEqual(filePath, "main", args, "Hello World");
    }

    [Test]
    public void assertPass() {
        string filePath = "../../../ProgramFiles/assertPass.prgm";
        int[] args = new int[] { };

        Tuple<IRCompUnit, IRCompUnit> bothIR = getBothVersionsOfIR(filePath);
        IRSimulator sim1 = new IRSimulator(bothIR.Item1);
        sim1.call("main", args);

        IRSimulator sim2 = new IRSimulator(bothIR.Item2);
        sim2.call("main", args);
    }

    [Test]
    public void assertFail() {
        string filePath = "../../../ProgramFiles/assertFail.prgm";
        int[] args = new int[] { };
        Tuple<IRCompUnit, IRCompUnit> bothIR = getBothVersionsOfIR(filePath);
        try {
            IRSimulator sim = new IRSimulator(bothIR.Item1);
            sim.call("main", args);
            Assert.Fail("The assertation passed. Not expected");
        } catch {
        }

        try {
            IRSimulator sim = new IRSimulator(bothIR.Item2);
            sim.call("main", args);
            Assert.Fail("The assertation passed. Not expected");
        } catch {
        }
    }

    [Test]
    public void unparseInt() {
        string filePath = "../../../ProgramFiles/unparseInt.prgm";
        int[] args = new int[] { };

        int[] expectedNums = {1, 100, -100};

        string expectedConsoleOutput = "";
        foreach(int expectedNum in expectedNums) {
            string expectedNumString = expectedNum.ToString();
            expectedConsoleOutput += expectedNumString + "\r\n";
        }
        ensureConsoleOutputIsEqual(filePath, "main", args, expectedConsoleOutput);
    }

    [Test]
    public void parseIntAndUnparseInt() {
        string filePath = "../../../ProgramFiles/parseIntAndUnparseInt.prgm";
        int[] args = new int[] { };

        Tuple<IRCompUnit, IRCompUnit> bothIR = getBothVersionsOfIR(filePath);
        IRSimulator sim1 = new IRSimulator(bothIR.Item1);
        sim1.call("main", args);

        IRSimulator sim2 = new IRSimulator(bothIR.Item2);
        sim2.call("main", args);
    }

    [Test]
    public void mutation() {
        string filePath = "../../../ProgramFiles/mutation.prgm";
        int[] args = new int[] { };
        ensureRetValsAreEqual(filePath, "main", args);
    }

    [Test]
    public void factorial() {
        string filePath = "../../../EndToEndTests/factorial.prgm";
        int[] args = new int[] {
            5
        };

        ensureRetValsAreEqual(filePath, "factorial_nonTail", args);
        ensureRetValsAreEqual(filePath, "factorial_tailWrap", args);
    }

    [Test]
    public void Binsearch() {
        string filePath = "../../../EndToEndTests/binsearch.prgm";
        int[] args = new int[] {};
        ensureConsoleOutputIsEqual(filePath, "main", args, "1");
    }

    [Test] 
    public void Collatz() {
        string filePath = "../../../EndToEndTests/collatz.prgm";
        int[] args = new int[] {};
        ensureConsoleOutputIsEqual(filePath, "main", args, "25");
    }  

    [Test]
    public void Sort() {
        string filePath = "../../../EndToEndTests/sort.prgm";
        int[] args = new int[] {};
        ensureConsoleOutputIsEqual(filePath, "main", args, "12345678910");
    }

    [Test]
    public void SimpleLoop() {
        string filePath = "../../../EndToEndTests/loop.prgm";
        int[] args = new int[] {};

        Tuple<IRCompUnit, IRCompUnit> bothIR = getBothVersionsOfIR(filePath);
        IRCompUnit originalIR = bothIR.Item1;
        IRCompUnit liftedIR = bothIR.Item2;

        IRSimulator sim1 = new IRSimulator(originalIR);
        IRSimulator sim2 = new IRSimulator(liftedIR);

        sim1.call("main", args);
        sim2.call("main", args);
    }

    [Test]
    public void Primes() {
        string filePath = "../../../EndToEndTests/primes.prgm";
        int[] args = new int[] {};
        ensureConsoleOutputIsEqual(filePath, "main", args, "97");
    }
}