using CompilerProj.Tokens;

namespace CompilerProj.Parse;
/*
 * Utilizing the expression grammars to determine which tokens are apart of the expression.
 * This will be utilized in ExprParser.cs for parsing the expression with shunting yard.
 * Why? The shunting yard algorithm requires the number of tokens to be known.
 */
internal sealed class ExprScanner {

    internal Queue<Token> exprTokens { get; }
    private Queue<Token> topLvlTokens;

    internal ExprScanner(Queue<Token> topLvlTokens) {
        this.exprTokens = new Queue<Token>();
        this.topLvlTokens = topLvlTokens;
    }

    /*
     * ⟨Expr⟩ ::= UNOP ⟨Expr⟩ ⟨exprFollowUp⟩
     * | ⟨BaseExpr ⟩ ⟨exprFollowUp⟩
     */
    internal void scanExpr() {
        switch (topLvlTokens.Peek().type) {
            //Resolve cases where minus isn't assigned context yet.
            case TokenType.minus:
                Token minusToken = topLvlTokens.Dequeue();
                exprTokens.Enqueue(
                    new Token(
                        minusToken.lexeme,
                        minusToken.line,
                        minusToken.column,
                        TokenType.minusNegation
                    )
                );
                scanExpr();
                scanExprFollowUp();
                break;
            case TokenType.minusNegation:
                consume(TokenType.minusNegation);
                scanExpr();
                scanExprFollowUp();
                break;
            case TokenType.not:
                consume(TokenType.not);
                scanExpr();
                scanExprFollowUp();
                break;
            case TokenType.startParen:
            case TokenType.identifier:
            case TokenType.number:
            case TokenType.reserved_true:
            case TokenType.reserved_false:
                scanBaseExpr();
                scanExprFollowUp();
                break;
            default:
                throw new Exception(
                    String.Format(
                        "Line {0}:{1}, Expected - ! <identifier> <number> true false, not {2}", 
                        topLvlTokens.Peek().line.ToString(), 
                        topLvlTokens.Peek().column.ToString(),
                        topLvlTokens.Peek().lexeme
                    )
                );
        }
    }

    /* ⟨exprFollowUp⟩ ::= BINOP ⟨Expr⟩ ⟨exprFollowUp⟩
     * | EPSILON
     */
    private void scanExprFollowUp() {
        switch(topLvlTokens.Peek().type) {
            //Resolve cases where minus isn't assigned context yet.
            case TokenType.minus:
                Token minusToken = topLvlTokens.Dequeue();
                exprTokens.Enqueue(
                    new Token(
                        minusToken.lexeme,
                        minusToken.line,
                        minusToken.column,
                        TokenType.minusSubtraction
                    )
                );
                break;
            case TokenType.minusSubtraction: consume(TokenType.minusSubtraction); break;
            case TokenType.plus: consume(TokenType.plus); break;
            case TokenType.multiply: consume(TokenType.multiply); break;
            case TokenType.divide: consume(TokenType.divide); break;
            case TokenType.modus: consume(TokenType.modus); break;
            case TokenType.less: consume(TokenType.less); break;
            case TokenType.greater: consume(TokenType.greater); break;
            case TokenType.lessThanEqual: consume(TokenType.lessThanEqual); break;
            case TokenType.greaterThanEqual: consume(TokenType.greaterThanEqual); break;
            case TokenType.equalTo: consume(TokenType.equalTo); break;
            case TokenType.notEqualTo: consume(TokenType.notEqualTo); break;
            case TokenType.and: consume(TokenType.and); break;
            case TokenType.or: consume(TokenType.or); break;
            default:
                return;
        }

        scanExpr();
        scanExprFollowUp();
    }

    /*
     * ⟨BaseExpr ⟩ ::= ‘(’ ⟨Expr⟩ ‘)’
     * | <IdentifierHeader>
     * | ⟨Lit⟩
     */
    private void scanBaseExpr() {
        switch(topLvlTokens.Peek().type) {
            case TokenType.startParen:
                consume(TokenType.startParen);
                scanExpr();
                consume(TokenType.endParen);
                break;
            case TokenType.identifier:
                scanIdentifierHeader();
                break;
            case TokenType.number:
            case TokenType.reserved_true:
            case TokenType.reserved_false:
                scanLiteral();
                break;
        }
    }

    /*
     * <IdentifierHeader> ::= <identifier> ⟨Access⟩
     * | <identifier> ⟨ProcedureCall ⟩
     */
    private void scanIdentifierHeader() {
        consume(TokenType.identifier);
        switch (topLvlTokens.Peek().type) {
            case TokenType.startParen:
                scanProcedureCallBody();
                break;
            default:
                scanAccess();
                break;
        }
    }

    /*
     * ⟨Access⟩ ::= ‘[’ ⟨Expr⟩ ‘]’ (‘[’ ⟨Expr⟩ ‘]’)?
     * | EPSILON
     */
    private void scanAccess() {
        if (topLvlTokens.Peek().type != TokenType.startBracket) {
            return;
        }

        consume(TokenType.startBracket);
        scanExpr();
        consume(TokenType.endBracket);

        if (topLvlTokens.Peek().type != TokenType.startBracket) {
            return;
        }

        consume(TokenType.startBracket);
        scanExpr();
        consume(TokenType.endBracket);
    }

    /*
     * ⟨ProcedureCallBody⟩ ::= ‘(’ ⟨ArgsOptional ⟩ ‘)’
     */
    private void scanProcedureCallBody() {
        consume(TokenType.startParen);
        scanArgsOptional();
        consume(TokenType.endParen);
    }

    /*
     * ⟨ArgsOptional ⟩ ::= ⟨Expr⟩ ⟨ArgsList⟩
     * | EPSILON
     */
    private void scanArgsOptional() {
        switch(topLvlTokens.Peek().type) {
            case TokenType.minus:
            case TokenType.minusNegation:
            case TokenType.not:
            case TokenType.identifier:
            case TokenType.number:
            case TokenType.reserved_true:
            case TokenType.reserved_false:
                scanExpr();
                scanArgsList();
                break;
            default:
                return;
        }
    }

    /*
     * ⟨ArgsList⟩ ::= ‘,’ ⟨Expr⟩ ⟨ArgsList⟩
     * | EPSILON
     */
    private void scanArgsList() {
        if (topLvlTokens.Peek().type != TokenType.comma) {
            return;
        }

        consume(TokenType.comma);
        scanExpr();
        scanArgsList();
    }

    /*
     * ⟨Lit⟩ ::= ⟨numberLiteral ⟩
     * | ⟨boolLiteral ⟩
     */
    private void scanLiteral() {
        switch(topLvlTokens.Peek().type) {
            case TokenType.number: consume(TokenType.number); break;
            case TokenType.reserved_true: consume(TokenType.reserved_true); break;
            case TokenType.reserved_false: consume(TokenType.reserved_false); break;
        }
    }

    private void consume(TokenType currTokenType) {
        Token expectedToken = topLvlTokens.Peek();
        if (expectedToken.type == currTokenType) {
            exprTokens.Enqueue(topLvlTokens.Dequeue());
        } else {
            throw new Exception(String.Format("Line {0}:{1}, The lexeme {2} does not match with the expected token {3}", 
                expectedToken.line.ToString(), expectedToken.column.ToString(), currTokenType.ToString(), expectedToken.type.ToString()
            ));
        }
    }
}