using System.Diagnostics;
using CompilerProj;

string filePath = "./ProgressTests/EndToEndTests/collatz.prgm";
IRCompUnit IR = Compiler.generateIR(filePath);
int[] arg = new int[] {};
IRSimulator simulator = new IRSimulator(IR);
simulator.call("main", arg);

// string filePath = "./ProgressTests/EndToEndTests/collatz.prgm";
// Compiler.compileFile(filePath);


