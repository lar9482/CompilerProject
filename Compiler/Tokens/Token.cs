namespace CompilerProj.Tokens;

public class Token {
    public string lexeme { get; }
    public int line { get; }
    public int column { get; }
    public TokenType type { get; }

    public Token(string lexeme, int line, int column, TokenType type) {
        this.lexeme = lexeme;
        this.line = line;
        this.column = column;
        this.type = type;
    }
}