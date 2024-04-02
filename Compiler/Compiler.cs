using CompilerProj.Lex;
using CompilerProj.Parse;
using CompilerProj.Tokens;

namespace CompilerProj;

public class Compiler {
    public static Queue<Token> dumpLex(string filePath) {
        StreamReader sr = new StreamReader(filePath);
        string programText = sr.ReadToEnd();
        sr.Close();
        
        Lexer lexer = new Lexer();
        Queue<Token> tokenQueue = lexer.lexProgram(programText);

        return tokenQueue;
    }

    public static ProgramAST dumpParse(string filePath) {
        Queue<Token> tokens = dumpLex(filePath);
        Parser parser = new Parser(tokens);
        
        return parser.parseProgram();
    }

    public static void compileFile(string filePath) {
        StreamReader sr = new StreamReader(filePath);
        string programText = sr.ReadToEnd();
        sr.Close();
        
        Lexer lexer = new Lexer();
        Queue<Token> tokenQueue = lexer.lexProgram(programText);

        Parser parser = new Parser(tokenQueue);
        ProgramAST programAST = parser.parseProgram();

        TypecheckerP1 topLvlDeclBuilder = new TypecheckerP1();
        topLvlDeclBuilder.visit(programAST);
        
        TypecheckerP2 topLvlDeclChecker = new TypecheckerP2();
        topLvlDeclChecker.visit(programAST);
        
        TypecheckerP3 functionChecker = new TypecheckerP3();
        functionChecker.visit(programAST);
        
        Console.WriteLine();
    }
}