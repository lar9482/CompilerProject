using CompilerProj.Lex;

namespace CompilerProj;

public class Compiler {
    
    public static void compileFile(string filePath) {
        StreamReader sr = new StreamReader(filePath);
        Lexer lexer = new Lexer();
        lexer.lexProgram(sr.ReadToEnd());
    }
}