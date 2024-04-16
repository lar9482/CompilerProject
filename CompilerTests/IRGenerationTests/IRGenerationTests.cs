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
        simulator.call("main", args);
        Console.WriteLine();
    }
}