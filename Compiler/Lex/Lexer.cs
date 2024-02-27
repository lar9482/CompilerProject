using System.Text.RegularExpressions;

using CompilerProj.Tokens;
namespace CompilerProj.Lex;

public class Lexer {
    private readonly Regex matchIdentifier;
    private readonly Regex matchNumber;
    private readonly Regex matchOneSymbol;
    private readonly Regex matchTwoSymbol;
    private readonly Regex matchWhitespace;

    private int lineCounter;
    private int columnCounter;

    public Lexer() {
        this.matchIdentifier = new Regex(@"\b^[a-zA-Z]{1}[a-zA-Z0-9_]*\b");
        this.matchNumber = new Regex(@"^\b\d+\b");
        this.matchOneSymbol = new Regex(@"^(\(|\{|\[|\]|\}|\)|,|;|=|\+|-|\*|\/|%|<|>|!|:)");
        this.matchTwoSymbol = new Regex(@"^(<=|>=|==|!=|&&|\|\|)");
        this.matchWhitespace = new Regex(@"^(\n|\t|\s|\r)");
    }

    public Queue<Token> lexProgram(string programText) {
        Queue<Token> tokenQueue = new Queue<Token>();

        while (programText.Length > 0) {
            Tuple<string, string> longestMatchWithType = scanLongestMatch(programText);
            string matchedLexeme = longestMatchWithType.Item1;
            string matchType = longestMatchWithType.Item2;

            switch(matchType) {
                case "matchIdentifier":
                    break;
                case "matchOneSymbol":
                    tokenQueue.Enqueue(resolveOneSymbol(matchedLexeme));
                    break;
                case "matchTwoSymbol":
                    tokenQueue.Enqueue(resolveTwoSymbol(matchedLexeme));
                    break;
                case "matchIntegers":
                    break;
                case "matchWhitespace":
                    if (matchedLexeme == "\n") {
                        columnCounter = 0;
                        lineCounter++;
                    } 
                    break;
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
}