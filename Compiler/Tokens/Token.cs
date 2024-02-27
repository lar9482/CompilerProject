namespace CompilerProj.Tokens;

public class Token {
    private string lexeme;
    private int line;
    private int column;
    private TokenType type;

    public Token(string lexeme, int line, int column, TokenType type) {
        this.lexeme = lexeme;
        this.line = line;
        this.column = column;
        this.type = type;
    }
}