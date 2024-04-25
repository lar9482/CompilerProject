using CompilerProj;

namespace CompilerTests;

public class IRGenerationTests {
    [SetUp]
    public void Setup() {}

    [Test]
    public void funcCall_multiReturn() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/funcCall_multiReturn.prgm";
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
        string filePath = "../../../IRGenerationTests/ProgramFiles/funcCall_singleReturn1.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { 10 };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(25));
    }

    [Test]
    public void funcCall_singleReturn2() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/funcCall_singleReturn2.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(600));
    }

    [Test]
    public void funcCall_singleReturn3() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/funcCall_singleReturn3.prgm";

        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(1000));
    }

    [Test]
    public void exprInteger() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/exprInteger.prgm";

        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(40));
    }

    [Test]
    public void arrayDecl_noInitialValues() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/arrayDecl_noInitialValues.prgm";

        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(150));
    }

    [Test]
    public void arrayDecl_hasInitialValues() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/arrayDecl_hasInitialValues.prgm";

        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(150));
    }

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
        IRCompUnit IR = Compiler.generateIR(filePath);

        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(15));
    }

    [Test]
    public void whileLoop_10Iterations() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/whileLoop_10Iterations.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(10));
    }

    [Test]
    public void whileLoop_0Iterations() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/whileLoop_0Iterations.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(0));
    }

    [Test]
    public void ifStmt() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/ifStmt.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(10));
    }

    [Test]
    public void if_elseif_stmts() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/if_elseif_stmts.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(0));
    }

    [Test]
    public void if_else_stmt() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/if_else_stmt.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(10));
    }

    [Test]
    public void if_elseif_else_stmts() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/if_elseif_else_stmts.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(0));
    }

    [Test]
    public void multiDimArrayDecl_noInitVals() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/multiDimArrayDecl_noInitVals.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(10));
    }

    [Test]
    public void multiDimArrayDecl_initVals() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/multiDimArrayDecl_initVals.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(10));
    }

    [Test]
    public void multiDimArrayDeclCallAST() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/multiDimArrayDeclCall.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(45));
    }
}