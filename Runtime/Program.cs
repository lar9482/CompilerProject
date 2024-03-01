using Compiler.Runtime;

// #################################################################
// CHANGE THESE TWO VARIABLES

string assemblyFilePath = "./AssemblyTests/loopGreaterThan.asm";
string binFilePath = "./AssemblyTests/Output/loopGreaterThan.out";

// #################################################################

int startProgramAddress = 0;
Assembler assembler = new Assembler(startProgramAddress);
assembler.assembleFile(assemblyFilePath, binFilePath);

Machine machine = new Machine(startProgramAddress);
machine.loadProgram(binFilePath);
machine.runProgram();