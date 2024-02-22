namespace Compiler.Runtime;

public struct Token {
    public string lexeme  { get; }
    public int lineCount  { get; }
    public TokenType type { get; }

    public Token(string lexeme, int lineCount, TokenType type) {
        this.lexeme = lexeme;
        this.lineCount = lineCount;
        this.type = type;
    }
}