using Compiler.Runtime;

namespace RuntimeTests;

public class Tests {

    private Assembler assembler;
    private Machine machine;

    [SetUp]
    public void Setup() {
        int startProgramAddress = 0;
        this.assembler = new Assembler(startProgramAddress);
        this.machine = new Machine(startProgramAddress);
    }

    [Test]
    public void RegisterTest() {
        string assemblyFilePath = "../../../TestFiles/regInst.asm";
        string binFilePath = "../../../TestFiles/Output/regInst.out";

        assembler.assembleFile(assemblyFilePath, binFilePath);
        machine.loadProgram(binFilePath);
        machine.runProgram();

        Dictionary<RegID, int> registers = machine.dumpRegisters();
        int expectedR1 = 1000;
        int expectedR2 = 1000;
        int expectedR3 = 1000;
        int expectedR4 = -1000;
        int expectedR5 = 2500;
        int expectedR6 = 480;
        int expectedR7 = 1020;
        int expectedR8 = 540;
        int expectedR9 = -1021;
        int expectedR10 = 12;
        int expectedR11 = 800;
        int expectedR12 = 3;
        int expectedRHI = 2;
        int expectedRLO = 500;
#pragma warning disable NUnit2005 // Consider using Assert.That(actual, Is.EqualTo(expected)) instead of Assert.AreEqual(expected, actual)
        Assert.AreEqual(expectedR1, registers[RegID.r1]);
        Assert.AreEqual(expectedR2, registers[RegID.r2]);
        Assert.AreEqual(expectedR3, registers[RegID.r3]);
        Assert.AreEqual(expectedR4, registers[RegID.r4]);
        Assert.AreEqual(expectedR5, registers[RegID.r5]);
        Assert.AreEqual(expectedR6, registers[RegID.r6]);
        Assert.AreEqual(expectedR7, registers[RegID.r7]);
        Assert.AreEqual(expectedR8, registers[RegID.r8]);
        Assert.AreEqual(expectedR9, registers[RegID.r9]);
        Assert.AreEqual(expectedR10, registers[RegID.r10]);
        Assert.AreEqual(expectedR11, registers[RegID.r11]);
        Assert.AreEqual(expectedR12, registers[RegID.r12]);
        Assert.AreEqual(expectedRHI, registers[RegID.rHI]);
        Assert.AreEqual(expectedRLO, registers[RegID.rLO]);
    }

    [Test]
    public void ImmediateTest() {
        string assemblyFilePath = "../../../TestFiles/immInst.asm";
        string binFilePath = "../../../TestFiles/Output/immInst.out";

        assembler.assembleFile(assemblyFilePath, binFilePath);
        machine.loadProgram(binFilePath);
        machine.runProgram();

        Dictionary<RegID, int> registers = machine.dumpRegisters();
        int expectedR1 = 900;
        int expectedR6 = 480;
        int expectedR7 = 1020;
        int expectedR8 = 540;
        int expectedR10 = 12;
        int expectedR11 = 800;
        int expectedRHI = 1800;
        int expectedRLO = 1800;

        Assert.AreEqual(expectedR1, registers[RegID.r1]);
        Assert.AreEqual(expectedR6, registers[RegID.r6]);
        Assert.AreEqual(expectedR7, registers[RegID.r7]);
        Assert.AreEqual(expectedR8, registers[RegID.r8]);
        Assert.AreEqual(expectedR10, registers[RegID.r10]);
        Assert.AreEqual(expectedR11, registers[RegID.r11]);
        Assert.AreEqual(expectedRHI, registers[RegID.rHI]);
        Assert.AreEqual(expectedRLO, registers[RegID.rLO]);
    }

    [Test]
    public void MemoryTest() {
        string assemblyFilePath = "../../../TestFiles/memInst.asm";
        string binFilePath = "../../../TestFiles/Output/memInst.out";

        assembler.assembleFile(assemblyFilePath, binFilePath);
        machine.loadProgram(binFilePath);
        machine.runProgram();

        Dictionary<RegID, int> registers = machine.dumpRegisters();

        // These registers were simply used to hold locations in memory
        int expectedR2 = 2000;
        int expectedR3 = 3000;
        int expectedR4 = 4000;
        int expectedR8 = 8000;
        int expectedR9 = 9000;
        int expectedR10 = 10000;

        //Using sw, all these registers should be equal.
        int expectedR1 = 1000;
        int expectedR5 = 1000;
        int expectedR6 = 1000;
        int expectedR7 = 1000;

        //Using sb, all these registers should be equal.
        int expectedR11 = 255;
        int expectedR12 = 255;
        int expectedR13 = 255;
        int expectedR14 = 255;

        Assert.AreEqual(expectedR1, registers[RegID.r1]);
        Assert.AreEqual(expectedR2, registers[RegID.r2]);
        Assert.AreEqual(expectedR3, registers[RegID.r3]);
        Assert.AreEqual(expectedR4, registers[RegID.r4]);
        Assert.AreEqual(expectedR5, registers[RegID.r5]);
        Assert.AreEqual(expectedR6, registers[RegID.r6]);
        Assert.AreEqual(expectedR7, registers[RegID.r7]);
        Assert.AreEqual(expectedR8, registers[RegID.r8]);
        Assert.AreEqual(expectedR9, registers[RegID.r9]);
        Assert.AreEqual(expectedR10, registers[RegID.r10]);
        Assert.AreEqual(expectedR11, registers[RegID.r11]);
        Assert.AreEqual(expectedR12, registers[RegID.r12]);
        Assert.AreEqual(expectedR13, registers[RegID.r13]);
        Assert.AreEqual(expectedR14, registers[RegID.r14]);
    }

    [Test]
    public void BranchTest() {
        string assemblyFilePath = "../../../TestFiles/branchInst.asm";
        string binFilePath = "../../../TestFiles/Output/branchInst.out";

        assembler.assembleFile(assemblyFilePath, binFilePath);
        machine.loadProgram(binFilePath);
        machine.runProgram();

        Dictionary<RegID, int> registers = machine.dumpRegisters();

        int expectedR1 = 100;
        int expectedR2 = 100;
        int expectedR3 = 300;
        int expectedR4 = 400;

        Assert.AreEqual(expectedR1, registers[RegID.r1]);
        Assert.AreEqual(expectedR2, registers[RegID.r2]);
        Assert.AreEqual(expectedR3, registers[RegID.r3]);
        Assert.AreEqual(expectedR4, registers[RegID.r4]);
    }

    [Test]
    public void JumpLinkTest() {
        string assemblyFilePath = "../../../TestFiles/jumpLink.asm";
        string binFilePath = "../../../TestFiles/Output/jumpLink.out";

        assembler.assembleFile(assemblyFilePath, binFilePath);
        machine.loadProgram(binFilePath);
        machine.runProgram();

        Dictionary<RegID, int> registers = machine.dumpRegisters();

        int expectedR1 = 10;
        int expectedR2 = 20;
        int expectedR3 = 30;
        int expectedR10 = 1000;
        int expectedR11 = 2000;
        int expectedR12 = 3000;

        Assert.AreEqual(expectedR1, registers[RegID.r1]);
        Assert.AreEqual(expectedR2, registers[RegID.r2]);
        Assert.AreEqual(expectedR3, registers[RegID.r3]);
        Assert.AreEqual(expectedR10, registers[RegID.r10]);
        Assert.AreEqual(expectedR11, registers[RegID.r11]);
        Assert.AreEqual(expectedR12, registers[RegID.r12]);
    }

    [Test]
    public void LoopLessThanTest() {
        string assemblyFilePath = "../../../TestFiles/loopLessThan.asm";
        string binFilePath = "../../../TestFiles/Output/loopLessthan.out";

        assembler.assembleFile(assemblyFilePath, binFilePath);
        machine.loadProgram(binFilePath);
        machine.runProgram();

        Dictionary<RegID, int> registers = machine.dumpRegisters();

        int expectedR1 = 101;
        int expectedR2 = 101;
        int expectedR3 = 5050;

        Assert.AreEqual(expectedR1, registers[RegID.r1]);
        Assert.AreEqual(expectedR2, registers[RegID.r2]);
        Assert.AreEqual(expectedR3, registers[RegID.r3]);
    }

    [Test]
    public void LoopGreaterThanTest() {
        string assemblyFilePath = "../../../TestFiles/loopGreaterThan.asm";
        string binFilePath = "../../../TestFiles/Output/loopGreaterthan.out";

        assembler.assembleFile(assemblyFilePath, binFilePath);
        machine.loadProgram(binFilePath);
        machine.runProgram();

        Dictionary<RegID, int> registers = machine.dumpRegisters();

        int expectedR1 = -1;
        int expectedR2 = -1;
        int expectedR3 = 5050;

        Assert.AreEqual(expectedR1, registers[RegID.r1]);
        Assert.AreEqual(expectedR2, registers[RegID.r2]);
        Assert.AreEqual(expectedR3, registers[RegID.r3]);
    }
}
#pragma warning restore NUnit2005 // Consider using Assert.That(actual, Is.EqualTo(expected)) instead of Assert.AreEqual(expected, actual)