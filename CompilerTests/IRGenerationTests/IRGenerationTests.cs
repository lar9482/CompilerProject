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

        // IRSimulator simulator = new IRSimulator(IR);
        // int retVal = simulator.call("b", args);
        // Assert.That(retVal, Is.EqualTo(12));
    }
}