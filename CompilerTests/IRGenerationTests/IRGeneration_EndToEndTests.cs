using CompilerProj;

namespace CompilerTests;

public class IRGeneration_EndToEndTests {
    [SetUp]
    public void Setup() {}

    [Test]
    public void IRGen_Factorial() {
        string filePath = "../../../EndToEndTests/factorial.prgm";
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

    [Test]
    public void IRGen_binsearch() {
        string filePath = "../../../EndToEndTests/binsearch.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] {};
        IRSimulator simulator = new IRSimulator(IR);
        simulator.call("main", args);

        string expectedConsoleOutput = "1";
        string actualConsoleOutput = simulator.consoleOutputCapture.ToString();

        Assert.That(actualConsoleOutput, Is.EqualTo(expectedConsoleOutput));
    }

    [Test] 
    public void IRGen_collatz() {
        string filePath = "../../../EndToEndTests/collatz.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] {};
        IRSimulator simulator = new IRSimulator(IR);
        simulator.call("main", args);

        string expectedConsoleOutput = "25";
        string actualConsoleOutput = simulator.consoleOutputCapture.ToString();
        Assert.That(actualConsoleOutput, Is.EqualTo(expectedConsoleOutput));
    }  

    [Test]
    public void IRGen_sort() {
        string filePath = "../../../EndToEndTests/sort.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] {};
        IRSimulator simulator = new IRSimulator(IR);
        simulator.call("main", args);

        string expectedConsoleOutput = "12345678910";
        string actualConsoleOutput = simulator.consoleOutputCapture.ToString();
        Assert.That(actualConsoleOutput, Is.EqualTo(expectedConsoleOutput));
    }

    [Test]
    public void IRGen_simpleLoop() {
        string filePath = "../../../EndToEndTests/loop.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] {};
        IRSimulator simulator = new IRSimulator(IR);
        simulator.call("main", args);
    }

    [Test]
    public void IRGen_primes() {
        string filePath = "../../../EndToEndTests/primes.prgm";
        IRCompUnit IR = Compiler.generateIR(filePath);
        int[] args = new int[] {};
        IRSimulator simulator = new IRSimulator(IR);
        simulator.call("main", args);

        string expectedConsoleOutput = "97";
        string actualConsoleOutput = simulator.consoleOutputCapture.ToString();
        Assert.That(actualConsoleOutput, Is.EqualTo(expectedConsoleOutput));
    }
}