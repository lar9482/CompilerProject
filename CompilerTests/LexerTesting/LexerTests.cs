using CompilerProj;
using CompilerProj.Tokens;

namespace CompilerTests;

public class LexerTests {
    [SetUp]
    public void Setup() {

    }

    private void compareExpectedAndActualTokens(Queue<TokenType> expectedTokenTypes, Queue<Token> actualTokens) {
        Token token = actualTokens.Dequeue();
        while (actualTokens.Count > 0) {
            if (token.type == expectedTokenTypes.Peek()) {
                expectedTokenTypes.Dequeue();
            }

            token = actualTokens.Dequeue();
        }
    }

    [Test]
    public void IdentifierTest() {
        string programFilePath = "../../../LexerTesting/identifier.prgm";
        Queue<TokenType> expectedTokenTypes = new Queue<TokenType>();
        expectedTokenTypes.Enqueue(TokenType.reserved_if);
        expectedTokenTypes.Enqueue(TokenType.reserved_while);
        expectedTokenTypes.Enqueue(TokenType.reserved_for);
        expectedTokenTypes.Enqueue(TokenType.reserved_else);
        expectedTokenTypes.Enqueue(TokenType.reserved_return);
        expectedTokenTypes.Enqueue(TokenType.identifier);
        expectedTokenTypes.Enqueue(TokenType.reserved_int);
        expectedTokenTypes.Enqueue(TokenType.reserved_bool);
        expectedTokenTypes.Enqueue(TokenType.reserved_true);
        expectedTokenTypes.Enqueue(TokenType.reserved_false);
        expectedTokenTypes.Enqueue(TokenType.identifier);
        expectedTokenTypes.Enqueue(TokenType.identifier);

        Queue<Token> actualTokens = Compiler.dumpLex(programFilePath);
        
        Assert.DoesNotThrow(() => {
            compareExpectedAndActualTokens(expectedTokenTypes, actualTokens);
        });
        Assert.IsTrue(expectedTokenTypes.Count == 0);
    }

    [Test]
    public void numberTest() {
        string programFilePath = "../../../LexerTesting/number.prgm";
        Queue<TokenType> expectedTokenTypes = new Queue<TokenType>();
        expectedTokenTypes.Enqueue(TokenType.number);
        expectedTokenTypes.Enqueue(TokenType.number);
        expectedTokenTypes.Enqueue(TokenType.minus);
        expectedTokenTypes.Enqueue(TokenType.number);

        Queue<Token> actualTokens = Compiler.dumpLex(programFilePath);
        
        Assert.DoesNotThrow(() => {
            compareExpectedAndActualTokens(expectedTokenTypes, actualTokens);
        });
        Assert.IsTrue(expectedTokenTypes.Count == 0);
    }

    [Test]
    public void oneSymbolTest() {
        string programFilePath = "../../../LexerTesting/oneSymbol.prgm";
        Queue<TokenType> expectedTokenTypes = new Queue<TokenType>();
        expectedTokenTypes.Enqueue(TokenType.startParen);
        expectedTokenTypes.Enqueue(TokenType.startCurly);
        expectedTokenTypes.Enqueue(TokenType.startBracket);
        expectedTokenTypes.Enqueue(TokenType.endBracket);
        expectedTokenTypes.Enqueue(TokenType.endCurly);
        expectedTokenTypes.Enqueue(TokenType.endParen);
        expectedTokenTypes.Enqueue(TokenType.comma);
        expectedTokenTypes.Enqueue(TokenType.semicolon);
        expectedTokenTypes.Enqueue(TokenType.assign);
        expectedTokenTypes.Enqueue(TokenType.plus);
        expectedTokenTypes.Enqueue(TokenType.minus);
        expectedTokenTypes.Enqueue(TokenType.multiply);
        expectedTokenTypes.Enqueue(TokenType.divide);
        expectedTokenTypes.Enqueue(TokenType.modus);
        expectedTokenTypes.Enqueue(TokenType.less);
        expectedTokenTypes.Enqueue(TokenType.greater);
        expectedTokenTypes.Enqueue(TokenType.not);
        expectedTokenTypes.Enqueue(TokenType.colon);

        Queue<Token> actualTokens = Compiler.dumpLex(programFilePath);
        
        Assert.DoesNotThrow(() => {
            compareExpectedAndActualTokens(expectedTokenTypes, actualTokens);
        });
        Assert.IsTrue(expectedTokenTypes.Count == 0);
    }

    [Test]
    public void twoSymbolsTest() {
        string programFilePath = "../../../LexerTesting/twoSymbol.prgm";
        Queue<TokenType> expectedTokenTypes = new Queue<TokenType>();
        expectedTokenTypes.Enqueue(TokenType.lessThanEqual);
        expectedTokenTypes.Enqueue(TokenType.greaterThanEqual);
        expectedTokenTypes.Enqueue(TokenType.equalTo);
        expectedTokenTypes.Enqueue(TokenType.notEqualTo);
        expectedTokenTypes.Enqueue(TokenType.and);
        expectedTokenTypes.Enqueue(TokenType.or);

        Queue<Token> actualTokens = Compiler.dumpLex(programFilePath);
        
        Assert.DoesNotThrow(() => {
            compareExpectedAndActualTokens(expectedTokenTypes, actualTokens);
        });
        Assert.IsTrue(expectedTokenTypes.Count == 0);
    }

    [Test]
    public void combinedSymbolsTest() {
        string programFilePath = "../../../LexerTesting/symbols.prgm";
        Queue<TokenType> expectedTokenTypes = new Queue<TokenType>();
        expectedTokenTypes.Enqueue(TokenType.startParen);
        expectedTokenTypes.Enqueue(TokenType.startCurly);
        expectedTokenTypes.Enqueue(TokenType.startBracket);
        expectedTokenTypes.Enqueue(TokenType.endBracket);
        expectedTokenTypes.Enqueue(TokenType.endCurly);
        expectedTokenTypes.Enqueue(TokenType.endParen);
        expectedTokenTypes.Enqueue(TokenType.comma);
        expectedTokenTypes.Enqueue(TokenType.semicolon);
        expectedTokenTypes.Enqueue(TokenType.assign);
        expectedTokenTypes.Enqueue(TokenType.plus);
        expectedTokenTypes.Enqueue(TokenType.minus);
        expectedTokenTypes.Enqueue(TokenType.multiply);
        expectedTokenTypes.Enqueue(TokenType.divide);
        expectedTokenTypes.Enqueue(TokenType.modus);
        expectedTokenTypes.Enqueue(TokenType.less);
        expectedTokenTypes.Enqueue(TokenType.greater);
        expectedTokenTypes.Enqueue(TokenType.not);
        expectedTokenTypes.Enqueue(TokenType.colon);

        expectedTokenTypes.Enqueue(TokenType.lessThanEqual);
        expectedTokenTypes.Enqueue(TokenType.greaterThanEqual);
        expectedTokenTypes.Enqueue(TokenType.equalTo);
        expectedTokenTypes.Enqueue(TokenType.notEqualTo);
        expectedTokenTypes.Enqueue(TokenType.and);
        expectedTokenTypes.Enqueue(TokenType.or);
        
        Queue<Token> actualTokens = Compiler.dumpLex(programFilePath);
        
        Assert.DoesNotThrow(() => {
            compareExpectedAndActualTokens(expectedTokenTypes, actualTokens);
        });
        Assert.IsTrue(expectedTokenTypes.Count == 0);
    }

    [Test] 
    public void charAndStringTest() {
        string programFilePath = "../../../LexerTesting/charAndString.prgm";
        Queue<TokenType> expectedTokenTypes = new Queue<TokenType>();
        expectedTokenTypes.Enqueue(TokenType.character);
        expectedTokenTypes.Enqueue(TokenType.character);
        expectedTokenTypes.Enqueue(TokenType.String);
        expectedTokenTypes.Enqueue(TokenType.String);
        
        Queue<Token> actualTokens = Compiler.dumpLex(programFilePath);
        
        Assert.DoesNotThrow(() => {
            compareExpectedAndActualTokens(expectedTokenTypes, actualTokens);
        });
        Assert.IsTrue(expectedTokenTypes.Count == 0);
    }
}