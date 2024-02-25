using Compiler.Runtime;

int startProgramAddress = 0;
string assemblyFilePath = "./AssemblyTests/loopGreaterThan.asm";
string binFilePath = "./AssemblyTests/Output/loopGreaterThan.out";

Assembler assembler = new Assembler(startProgramAddress);
assembler.assembleFile(assemblyFilePath, binFilePath);

Machine machine = new Machine(startProgramAddress);
machine.loadProgram(binFilePath);
machine.runProgram();
Console.WriteLine();