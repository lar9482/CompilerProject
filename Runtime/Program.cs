using Compiler.Runtime;

int startProgramAddress = 0;
string assemblyFilePath = "./AssemblyTests/printInst.asm";
string binFilePath = "./AssemblyTests/Output/printInst.out";

Assembler assembler = new Assembler(startProgramAddress);
assembler.assembleFile(assemblyFilePath, binFilePath);

Machine machine = new Machine(startProgramAddress);
machine.loadProgram(binFilePath);
machine.runProgram();
Console.WriteLine();