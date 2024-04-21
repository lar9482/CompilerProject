using CompilerProj;

namespace CompilerTests;

public class IRGeneration_EndToEndTests {
    [SetUp]
    public void Setup() {}

    [Test]
    public void IRGen_Factorial() {
        string filePath = "../../../IRGenerationTests/EndToEndTests/factorial.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] {
            5
        };

        IRSimulator simulator = new IRSimulator(IR);
        int nonTailRetVal = simulator.call("factorial_nonTail", args);
        int tailRetVal = simulator.call("factorial_tailWrap", args);
        
        Assert.That(nonTailRetVal, Is.EqualTo(120));
        Assert.That(tailRetVal, Is.EqualTo(120));
    }
}