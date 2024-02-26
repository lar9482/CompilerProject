namespace CompilerProj.Tokens;

public class Token {
    private string lexeme;
    private int line;
    private TokenType type;

    public Token(string lexeme, int line, TokenType type) {
        this.lexeme = lexeme;
        this.line = line;
        this.type = type;
    }
}