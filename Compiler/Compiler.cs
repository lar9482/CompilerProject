using CompilerProj.Lex;
using CompilerProj.Tokens;

namespace CompilerProj;

public class Compiler {
    
    public static void compileFile(string filePath) {
        StreamReader sr = new StreamReader(filePath);

        Lexer lexer = new Lexer();
        Queue<Token> tokenQueue = lexer.lexProgram(sr.ReadToEnd());

        Console.WriteLine();
    }
}