namespace CompilerProj.Tokens;

internal sealed class Token {
    internal string lexeme { get; }
    internal int line { get; }
    internal int column { get; }
    internal TokenType type { get; }

    internal Token(string lexeme, int line, int column, TokenType type) {
        this.lexeme = lexeme;
        this.line = line;
        this.column = column;
        this.type = type;
    }
}