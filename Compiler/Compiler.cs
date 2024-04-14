using CompilerProj.Lex;
using CompilerProj.Parse;
using CompilerProj.Tokens;

namespace CompilerProj;

public class Compiler {

    public static string readFile(string filePath) {
        StreamReader sr = new StreamReader(filePath);
        string programText = sr.ReadToEnd();
        sr.Close();

        return programText;
    }

    public static Queue<Token> lex(string filePath) {
        string programText = readFile(filePath);
        
        Lexer lexer = new Lexer();
        Queue<Token> tokenQueue = lexer.lexProgram(programText);

        return tokenQueue;
    }

    public static ProgramAST parse(string filePath) {
        Queue<Token> tokens = lex(filePath);
        Parser parser = new Parser(tokens);
        
        return parser.parseProgram();
    }

    public static Tuple<ProgramAST, List<string>> typecheck(string filePath) {
        ProgramAST programAST = parse(filePath);

        TypecheckerP1 topLvlDeclBuilder = new TypecheckerP1();
        topLvlDeclBuilder.visit(programAST);
        
        TypecheckerP2 topLvlDeclChecker = new TypecheckerP2();
        topLvlDeclChecker.visit(programAST);
        
        TypecheckerP3 functionChecker = new TypecheckerP3();
        functionChecker.visit(programAST);

        List<string> allErrorMsgs = new List<string>();
        allErrorMsgs.AddRange(topLvlDeclBuilder.errorMsgs);
        allErrorMsgs.AddRange(topLvlDeclChecker.errorMsgs);
        allErrorMsgs.AddRange(functionChecker.errorMsgs);

        return Tuple.Create<ProgramAST, List<string>>(programAST, allErrorMsgs);
    }

    public static ProgramAST ensureNoSemanticErrors(string filePath) {
        Tuple<ProgramAST, List<string>> ASTWithErrors = typecheck(filePath);
        ProgramAST programAST = ASTWithErrors.Item1;
        List<string> semanticErrors = ASTWithErrors.Item2;

        if (semanticErrors.Count > 0) {
            throw new Exception(
                string.Join(Environment.NewLine, semanticErrors)
            );
        }

        return programAST;
    }

    public static void generateIR(string filePath) {
        ProgramAST checkedAST = ensureNoSemanticErrors(filePath);
        IRGenerator generator = new IRGenerator();

        IRCompUnit irProgram = generator.visit<IRCompUnit>(checkedAST);

        Console.WriteLine();
    }

    public static void compileFile(string filePath) {
        generateIR(filePath);
    }
}