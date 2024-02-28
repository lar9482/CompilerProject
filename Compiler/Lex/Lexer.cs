using System.Text.RegularExpressions;

using CompilerProj.Tokens;
namespace CompilerProj.Lex;

public class Lexer {
    private readonly Regex matchIdentifier;
    private readonly Regex matchNumber;
    private readonly Regex matchOneSymbol;
    private readonly Regex matchTwoSymbol;
    private readonly Regex matchWhitespace;
    private readonly Regex matchComment;

    private int lineCounter;
    private int columnCounter;

    public Lexer() {
        this.matchIdentifier = new Regex(@"\b^[a-zA-Z]{1}[a-zA-Z0-9_]*\b");
        this.matchNumber = new Regex(@"^\b\d+\b");
        this.matchOneSymbol = new Regex(@"^(\(|\{|\[|\]|\}|\)|,|;|=|\+|-|\*|\/|%|<|>|!|:|\"")");
        this.matchTwoSymbol = new Regex(@"^(<=|>=|==|!=|&&|\|\|)");
        this.matchWhitespace = new Regex(@"^(\n|\t|\s|\r)");
        this.matchComment = new Regex(@"^//");
        this.columnCounter = 1;
        this.lineCounter = 1;
    }

    public Queue<Token> lexProgram(string programText) {
        Queue<Token> tokenQueue = new Queue<Token>();

        while (programText.Length > 0) {
            Tuple<string, string> longestMatchWithType = scanLongestMatch(programText);
            string matchedLexeme = longestMatchWithType.Item1;
            string matchType = longestMatchWithType.Item2;

            switch(matchType) {
                case "matchIdentifier":
                    tokenQueue.Enqueue(resolveIdentifer(matchedLexeme));
                    break;
                case "matchOneSymbol":
                    tokenQueue.Enqueue(resolveOneSymbol(matchedLexeme));
                    break;
                case "matchTwoSymbol":
                    tokenQueue.Enqueue(resolveTwoSymbol(matchedLexeme));
                    break;
                case "matchNumber":
                    Token numberToken = new Token(matchedLexeme, lineCounter, columnCounter, TokenType.number);
                    tokenQueue.Enqueue(numberToken);
                    break;
                case "matchWhitespace":
                    if (matchedLexeme == "\n") {
                        columnCounter = 1;
                        lineCounter++;
                    } 
                    break;
                case "matchComment":
                    int newlineIndex = programText.IndexOf("\n");
                    programText = programText.Remove(0, newlineIndex);
                    continue;
                default:
                    throw new InvalidOperationException(
                        String.Format("Lexer: {0} is not a recognizable lexeme", matchedLexeme)
                    );
            }
            columnCounter += matchedLexeme.Length;
            programText = programText.Remove(0, matchedLexeme.Length);
        }

        return tokenQueue;
    }

    private Tuple<string, string> scanLongestMatch(string programText) {
        Dictionary<string, string> matches = new Dictionary<string, string>();
        matches.Add("matchIdentifier", matchIdentifier.Match(programText).Value);
        matches.Add("matchNumber", matchNumber.Match(programText).Value);
        matches.Add("matchTwoSymbol", matchTwoSymbol.Match(programText).Value);
        matches.Add("matchOneSymbol", matchOneSymbol.Match(programText).Value);
        matches.Add("matchWhitespace", matchWhitespace.Match(programText).Value);
        matches.Add("matchComment", matchComment.Match(programText).Value);
        int longestMatchLength = 0;
        string longestMatch = "";
        string matchType = "";

        foreach(KeyValuePair<string, string> regexProgramMatch in matches) {
            if (regexProgramMatch.Value.Length > longestMatchLength) {
                longestMatch = regexProgramMatch.Value;
                matchType = regexProgramMatch.Key;
                longestMatchLength = regexProgramMatch.Value.Length;
            }
        }

        return Tuple.Create<string, string>(longestMatch, matchType);
    }

    private Token resolveOneSymbol(string lexeme) {
        switch(lexeme) {
            case "(": return new Token(lexeme, lineCounter, columnCounter, TokenType.startParen); 
            case "{": return new Token(lexeme, lineCounter, columnCounter, TokenType.startCurly); 
            case "[": return new Token(lexeme, lineCounter, columnCounter, TokenType.startBracket); 
            case "]": return new Token(lexeme, lineCounter, columnCounter, TokenType.endBracket); 
            case "}": return new Token(lexeme, lineCounter, columnCounter, TokenType.endCurly);
            case ")": return new Token(lexeme, lineCounter, columnCounter, TokenType.endParen);
            case ",": return new Token(lexeme, lineCounter, columnCounter, TokenType.comma);
            case ";": return new Token(lexeme, lineCounter, columnCounter, TokenType.semicolon);
            case "=": return new Token(lexeme, lineCounter, columnCounter, TokenType.assign);
            case "+": return new Token(lexeme, lineCounter, columnCounter, TokenType.plus);
            case "-": return new Token(lexeme, lineCounter, columnCounter, TokenType.minus);
            case "*": return new Token(lexeme, lineCounter, columnCounter, TokenType.multiply);
            case "/": return new Token(lexeme, lineCounter, columnCounter, TokenType.divide);
            case "%": return new Token(lexeme, lineCounter, columnCounter, TokenType.modus);
            case "<": return new Token(lexeme, lineCounter, columnCounter, TokenType.less);
            case ">": return new Token(lexeme, lineCounter, columnCounter, TokenType.greater);
            case "!": return new Token(lexeme, lineCounter, columnCounter, TokenType.not);
            case ":": return new Token(lexeme, lineCounter, columnCounter, TokenType.colon);
            case "\"": return new Token(lexeme, lineCounter, columnCounter, TokenType.doubleQuotes);
            default:
                throw new Exception(
                    String.Format("Lexer: {0} is not a recognizable one symbol", lexeme)
                );
        }
    }

    private Token resolveTwoSymbol(string lexeme) {
        switch (lexeme) {
            case "<=": return new Token(lexeme, lineCounter, columnCounter, TokenType.lessThanEqual); 
            case ">=": return new Token(lexeme, lineCounter, columnCounter, TokenType.greaterThanEqual); 
            case "==": return new Token(lexeme, lineCounter, columnCounter, TokenType.equalTo); 
            case "!=": return new Token(lexeme, lineCounter, columnCounter, TokenType.notEqualTo); 
            case "&&": return new Token(lexeme, lineCounter, columnCounter, TokenType.and); 
            case "||": return new Token(lexeme, lineCounter, columnCounter, TokenType.or);
            default:
                throw new Exception(
                    String.Format("Lexer: {0} is not a recognizable two symbol", lexeme)
                );
        }
        
    }

    private Token resolveIdentifer(string lexeme) {
        switch(lexeme) {
            case "if": return new Token(lexeme, lineCounter, columnCounter, TokenType.reserved_if);
            case "while": return new Token(lexeme, lineCounter, columnCounter, TokenType.reserved_while);
            case "for": return new Token(lexeme, lineCounter, columnCounter, TokenType.reserved_for);
            case "else": return new Token(lexeme, lineCounter, columnCounter, TokenType.reserved_else);
            case "return": return new Token(lexeme, lineCounter, columnCounter, TokenType.reserved_return); 
            case "length": return new Token(lexeme, lineCounter, columnCounter, TokenType.reserved_length);
            case "int": return new Token(lexeme, lineCounter, columnCounter, TokenType.reserved_int);
            case "bool": return new Token(lexeme, lineCounter, columnCounter, TokenType.reserved_bool);
            case "true": return new Token(lexeme, lineCounter, columnCounter, TokenType.reserved_true);
            case "false": return new Token(lexeme, lineCounter, columnCounter, TokenType.reserved_false);
            default: return new Token(lexeme, lineCounter, columnCounter, TokenType.identifier);
        }
    }
}