using CompilerProj;

namespace CompilerTests;

/*
 * The general layout for these tests are as follows:
 *  1. Take the program located at 'filePath' and generate IR.
 *  2. Specify possible arguments that you want to pass into a function that was specified in the program.
 *  3. Call a function using the IRSimulator to interpret the IR.
 *  4. Either
 *   - Check the return value
 *   - Check the print/println output
 *   - Simply ensure the program didn't crash.
 */
public class IRGenerationTests {
    [SetUp]
    public void Setup() {}

    [Test]
    public void funcCall_multiReturn() {
        string filePath = "../../../ProgramFiles/funcCall_multiReturn.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] {
            2, 1
        };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("b", args);
        Assert.That(retVal, Is.EqualTo(12));
    }

    [Test]
    public void funcCall_singleReturn1() {
        string filePath = "../../../ProgramFiles/funcCall_singleReturn1.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { 10 };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(25));
    }

    [Test]
    public void funcCall_singleReturn2() {
        string filePath = "../../../ProgramFiles/funcCall_singleReturn2.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(600));
    }

    [Test]
    public void funcCall_singleReturn3() {
        string filePath = "../../../ProgramFiles/funcCall_singleReturn3.prgm";

        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(1000));
    }

    [Test]
    public void exprInteger() {
        string filePath = "../../../ProgramFiles/exprInteger.prgm";

        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(40));
    }

    [Test]
    public void exprBool() {
        string filePath = "../../../ProgramFiles/exprBool.prgm";

        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(1));
    }

    [Test]
    public void arrayDecl_noInitialValues() {
        string filePath = "../../../ProgramFiles/arrayDecl_noInitialValues.prgm";

        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(0));
    }

    [Test]
    public void arrayDecl_noInitialValues_thenAssigned() {
        string filePath = "../../../ProgramFiles/arrayDecl_noInitialValues_thenAssigned.prgm";

        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(150));
    }

    [Test]
    public void arrayDecl_hasInitialValues() {
        string filePath = "../../../ProgramFiles/arrayDecl_hasInitialValues.prgm";

        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(150));
    }

    [Test]
    public void arrayDecl_outOfBounds_Positive() {
        string filePath = "../../../ProgramFiles/arrayDecl_outOfBounds_Positive.prgm";
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
    public void arrayDecl_outOfBounds_Negative() {
        string filePath = "../../../ProgramFiles/arrayDecl_outOfBounds_Negative.prgm";
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
        string filePath = "../../../ProgramFiles/arrayDeclCall.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);

        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(15));
    }

    [Test]
    public void whileLoop_10Iterations() {
        string filePath = "../../../ProgramFiles/whileLoop_10Iterations.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(10));
    }

    [Test]
    public void whileLoop_0Iterations() {
        string filePath = "../../../ProgramFiles/whileLoop_0Iterations.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(0));
    }

    [Test]
    public void forLoopSimple() {
        string filePath = "../../../ProgramFiles/forLoopSimple.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };
        IRSimulator simulator = new IRSimulator(IR);
        simulator.call("main", args);

        string expectedConsoleOutput = "10";
        string actualConsoleOutput = simulator.consoleOutputCapture.ToString();
        Assert.That(actualConsoleOutput, Is.EqualTo(expectedConsoleOutput));
    }

    [Test]
    public void forLoop_DeclsInit() {
        string filePath = "../../../ProgramFiles/forLoop_DeclsInit.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };
        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);

        Assert.That(retVal, Is.EqualTo(50));
    }

    [Test]
    public void forLoop_NonDeclsInit() {
        string filePath = "../../../ProgramFiles/forLoop_NonDeclsInit.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };
        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);

        Assert.That(retVal, Is.EqualTo(50));
    }

    [Test]
    public void ifStmt() {
        string filePath = "../../../ProgramFiles/ifStmt.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(10));
    }

    [Test]
    public void if_elseif_stmts() {
        string filePath = "../../../ProgramFiles/if_elseif_stmts.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(0));
    }

    [Test]
    public void if_else_stmt() {
        string filePath = "../../../ProgramFiles/if_else_stmt.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(0));
    }

    [Test]
    public void if_elseif_else_stmts() {
        string filePath = "../../../ProgramFiles/if_elseif_else_stmts.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(0));
    }

    [Test]
    public void multiDimArrayDecl_noInitVals() {
        string filePath = "../../../ProgramFiles/multiDimArrayDecl_noInitVals.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(0));
    }

    [Test]
    public void multiDimArrayDecl_noInitVals_thenAssign() {
        string filePath = "../../../ProgramFiles/multiDimArrayDecl_noInitVals_thenAssign.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(10));
    }

    [Test]
    public void multiDimArrayDecl_initVals() {
        string filePath = "../../../ProgramFiles/multiDimArrayDecl_initVals.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(10));
    }

    [Test]
    public void multiDimArrayDeclCallAST() {
        string filePath = "../../../ProgramFiles/multiDimArrayDeclCall.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(45));
    }

    [Test]
    public void printlnHelloWorld() {
        string filePath = "../../../ProgramFiles/printlnHelloWorld.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        simulator.call("main", args);

        string expectedConsoleOutput = "Hello World\r\n";
        string actualConsoleOutput = simulator.consoleOutputCapture.ToString();
        Assert.That(actualConsoleOutput, Is.EqualTo(expectedConsoleOutput));
    }

    [Test]
    public void printHelloWorld() {
        string filePath = "../../../ProgramFiles/printHelloWorld.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        simulator.call("main", args);

        string expectedConsoleOutput = "Hello World";
        string actualConsoleOutput = simulator.consoleOutputCapture.ToString();
        Assert.That(actualConsoleOutput, Is.EqualTo(expectedConsoleOutput));
    }

    [Test]
    public void assertPass() {
        string filePath = "../../../ProgramFiles/assertPass.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        simulator.call("main", args);
    }

    [Test]
    public void assertFail() {
        string filePath = "../../../ProgramFiles/assertFail.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        try {
            simulator.call("main", args);
            Assert.Fail("The assertation passed. Not expected");
        } catch {
        }
    }

    [Test]
    public void unparseInt() {
        string filePath = "../../../ProgramFiles/unparseInt.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };
        IRSimulator simulator = new IRSimulator(IR);
        simulator.call("main", args);

        int[] expectedNums = {1, 100, -100};

        string expectedConsoleOutput = "";
        foreach(int expectedNum in expectedNums) {
            string expectedNumString = expectedNum.ToString();
            expectedConsoleOutput += expectedNumString + "\r\n";
        }
        string actualConsoleOutput = simulator.consoleOutputCapture.ToString();
        Assert.That(actualConsoleOutput, Is.EqualTo(expectedConsoleOutput));
    }

    [Test]
    public void parseIntAndUnparseInt() {
        string filePath = "../../../ProgramFiles/parseIntAndUnparseInt.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };
        IRSimulator simulator = new IRSimulator(IR);
        simulator.call("main", args);
    }

    [Test]
    public void mutation() {
        string filePath = "../../../ProgramFiles/mutation.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };
        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);

        Assert.That(retVal, Is.EqualTo(6));
    }
}