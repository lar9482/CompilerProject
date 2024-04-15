using CompilerProj;

namespace CompilerTests;

public class IRGenerationTests {
    [SetUp]
    public void Setup() {}

    [Test]
    public void test1() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/test1.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] {
            2, 1
        };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("b", args);
        Assert.That(retVal, Is.EqualTo(12));
    }

    [Test]
    public void test2() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/test2.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { 10 };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(25));
    }

    [Test]
    public void test3() {
        string filePath = "../../../IRGenerationTests/ProgramFiles/test3.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] { };

        IRSimulator simulator = new IRSimulator(IR);
        int retVal = simulator.call("main", args);
        Assert.That(retVal, Is.EqualTo(600));
    }
}