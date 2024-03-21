using CompilerProj.Tokens;

namespace CompilerProj.Parse;

/*
 * The parsing routine for expressions using shunting yard.
 * This will factor in operator precedence.
 */
public sealed class ExprParser {
    private Queue<Token> exprTokens;

    public ExprParser(Queue<Token> topLvlTokens) {
        ExprScanner scanner = new ExprScanner(topLvlTokens);
        scanner.scanExpr();

        this.exprTokens = scanner.exprTokens;
    }

    public ExprAST parseByShuntingYard() {
        Stack<ExprAST> operandStack = new Stack<ExprAST>();
        Stack<Token> operatorStack = new Stack<Token>();

        while (exprTokens.Count > 0) {
            if (isStartOfOperand(exprTokens.Peek())) {
                ExprAST operand = parseOperand();
                operandStack.Push(operand);

            } else if (isOperator(exprTokens.Peek())){
                while (
                    operatorStack.Count > 0 &&
                    isOperator(operatorStack.Peek()) &&
                    (getOperatorPrecedence(operatorStack.Peek()) <= getOperatorPrecedence(exprTokens.Peek()))
                ) {
                    Token higherOrderOperator = operatorStack.Pop();
                    parseInternalNode(operandStack, higherOrderOperator);
                }

                operatorStack.Push(exprTokens.Peek());
                exprTokens.Dequeue();

            } else if (exprTokens.Peek().type == TokenType.startParen) {
                operatorStack.Push(exprTokens.Peek());
                exprTokens.Dequeue();
            } else if (exprTokens.Peek().type == TokenType.endParen) {
                while (operatorStack.Count > 0 && operatorStack.Peek().type != TokenType.startParen) {
                    Token operatorToken = operatorStack.Pop();
                    parseInternalNode(operandStack, operatorToken);
                }
                operatorStack.Pop();
                exprTokens.Dequeue();
            }
        }
        while (operatorStack.Count > 0) {
            Token operatorToken = operatorStack.Pop();
            parseInternalNode(operandStack, operatorToken);
        }
        
        return operandStack.Pop();
    }

    private void parseInternalNode(Stack<ExprAST> operandStack, Token operatorToken) {
        if (isUnaryOperator(operatorToken)) {
            ExprAST operand = operandStack.Pop();
            UnaryExprAST unaryExpr = new UnaryExprAST(
                operand,
                getUnaryType(operatorToken),
                operatorToken.line,
                operatorToken.column
            );

            operandStack.Push(unaryExpr);
        } else if (isBinaryOperator(operatorToken)) {
            ExprAST rightOperand = operandStack.Pop();
            ExprAST leftOperand = operandStack.Pop();

            BinaryExprAST binaryExpr = new BinaryExprAST(
                leftOperand, rightOperand,
                getBinaryType(operatorToken),
                leftOperand.lineNumber,
                leftOperand.columnNumber
            );
            operandStack.Push(binaryExpr);
        }
    }
    /*
     * ⟨Operand ⟩ ::= <IdentifierHeader>
     * | ⟨Lit⟩
     */
    private ExprAST parseOperand() {
        switch(exprTokens.Peek().type) {
            case TokenType.identifier:
                return parseAccessOrProcedureCall();
            case TokenType.number:
            case TokenType.reserved_true: 
            case TokenType.reserved_false:
                return parseLiteral();
            default:
                throw new Exception(
                    String.Format("Line {0}:{1}, Expected <identifier> <number> true false, not {2}",
                        exprTokens.Peek().line, exprTokens.Peek().column, exprTokens.Peek().lexeme
                    )
                );
        }
    }

    /*
     * <ArrayOrProcedureCall> ::= <identifier> ⟨Access⟩
     * | <identifier> ⟨ProcedureCall ⟩
     */
    private ExprAST parseAccessOrProcedureCall() {
        Token identifierToken = consume(TokenType.identifier);
        switch(exprTokens.Peek().type) {
            case TokenType.startParen:
                return parseProcedureCall(identifierToken);
            default:
                return parseAccess(identifierToken);
        }
    }

    /*
     * ⟨Access⟩ ::= ‘[’ ⟨Expr⟩ ‘]’ (‘[’ ⟨Expr⟩ ‘]’)?
     * | EPSILON
     */
    private ExprAST parseAccess(Token identifier) {
        if (exprTokens.Peek().type != TokenType.startBracket) {
            return new VarAccessAST(
                identifier.lexeme,
                identifier.line,
                identifier.column
            );
        }

        consume(TokenType.startBracket);
        ExprParser firstParser = new ExprParser(exprTokens);
        ExprAST firstAccess = firstParser.parseByShuntingYard();
        consume(TokenType.endBracket);

        if (exprTokens.Peek().type != TokenType.startBracket) {
            return new ArrayAccessAST(
                identifier.lexeme,
                firstAccess,
                identifier.line,
                identifier.column
            );
        }

        consume(TokenType.startBracket);
        ExprParser secondParser = new ExprParser(exprTokens);
        ExprAST secondAccess = secondParser.parseByShuntingYard();
        consume(TokenType.endBracket);

        return new MultiDimArrayAccessAST(
            identifier.lexeme,
            firstAccess,
            secondAccess,
            identifier.line,
            identifier.column
        );
    }

    /*
     * ⟨ProcedureCallBody⟩ ::= ‘(’ ⟨ArgsOptional ⟩ ‘)’
     */
    private ProcedureCallAST parseProcedureCall(Token identifier) {
        consume(TokenType.startParen);
        List<ExprAST> args = parseArgsOptional();
        consume(TokenType.endParen);

        return new ProcedureCallAST(
            identifier.lexeme,
            args,
            identifier.line,
            identifier.column
        );
    }


    /*
     * ⟨ArgsOptional ⟩ ::= ⟨Expr⟩ ⟨ArgsList⟩
     * | EPSILON
     */
    private List<ExprAST> parseArgsOptional() {
        List<ExprAST> args = new List<ExprAST>();

        switch(exprTokens.Peek().type) {
            case TokenType.minus:
            case TokenType.minusNegation:
            case TokenType.not:
            case TokenType.identifier:
            case TokenType.number:
            case TokenType.reserved_true:
            case TokenType.reserved_false:
                ExprParser exprParser = new ExprParser(exprTokens);
                ExprAST arg = exprParser.parseByShuntingYard();
                args.Add(arg);

                List<ExprAST> nextArgs = parseArgsList();
                return args.Concat<ExprAST>(nextArgs).ToList<ExprAST>();
            default:
                return args;
        }
    }

    /*
     * ⟨ArgsList⟩ ::= ‘,’ ⟨Expr⟩ ⟨ArgsList⟩
     * | EPSILON
     */
    private List<ExprAST> parseArgsList() {
        List<ExprAST> args = new List<ExprAST>();
        if (exprTokens.Peek().type != TokenType.comma) {
            return args;
        }

        consume(TokenType.comma);
        ExprParser exprParser = new ExprParser(exprTokens);
        ExprAST expr = exprParser.parseByShuntingYard();
        args.Add(expr);

        List<ExprAST> nextArgs = parseArgsList();

        return args.Concat<ExprAST>(nextArgs).ToList();
    }

    /*
     * ⟨Literal⟩ ::= <number>
     * | true
     * | false
     */
    private ExprAST parseLiteral() {
        switch(exprTokens.Peek().type) {
            case TokenType.number:
                Token numberToken = consume(TokenType.number);
                return new IntLiteralAST(
                    Int32.Parse(numberToken.lexeme),
                    numberToken.line,
                    numberToken.column
                );
            case TokenType.reserved_true: 
            case TokenType.reserved_false:
                Token boolToken = consume(exprTokens.Peek().type);
                return new BoolLiteralAST(
                    boolToken.lexeme == "true",
                    boolToken.line,
                    boolToken.column
                );
            default:
                throw new Exception(
                    String.Format("Line {0}:{1}, Expected <number> true false, not {2}",
                        exprTokens.Peek().line, exprTokens.Peek().column, exprTokens.Peek().lexeme
                    )
                );
        }   
    }

    private bool isStartOfOperand(Token token) {
        switch(token.type) {
            case TokenType.identifier:
            case TokenType.number:
            case TokenType.reserved_true:
            case TokenType.reserved_false:
                return true;
            default:
                return false;
        }
    }

    private bool isOperator(Token token) {
        return (
            isBinaryOperator(token) || isUnaryOperator(token)
        );
    }

    private bool isBinaryOperator(Token token) {
        switch(token.type) {
            case TokenType.plus:
            case TokenType.minusSubtraction:
            case TokenType.multiply:
            case TokenType.divide:
            case TokenType.modus: 
            case TokenType.less:
            case TokenType.greater: 
            case TokenType.lessThanEqual:
            case TokenType.greaterThanEqual:
            case TokenType.equalTo:
            case TokenType.notEqualTo:
            case TokenType.and: 
            case TokenType.or:
                return true;
            default:
                return false;
        }
    }

    private bool isUnaryOperator(Token token) {
        switch (token.type) {
            case TokenType.not:
            case TokenType.minusNegation:
                return true;
            default:
                return false;
        }
    }

    private UnaryExprType getUnaryType(Token token) {
        switch(token.type) {
            case TokenType.not: return UnaryExprType.NOT;
            case TokenType.minusNegation: return UnaryExprType.NEGATE;
            default: 
                throw new Exception(
                    String.Format("Line {0}:{1}, Expected ! -, not {2}",
                        exprTokens.Peek().line, exprTokens.Peek().column, exprTokens.Peek().lexeme
                    )
                );
        }
    }

    private BinaryExprType getBinaryType(Token token) {
        switch(token.type) {
            case TokenType.plus: return BinaryExprType.ADD;
            case TokenType.minusSubtraction: return BinaryExprType.SUB;
            case TokenType.multiply:return BinaryExprType.MULT;
            case TokenType.divide:return BinaryExprType.DIV;
            case TokenType.modus: return BinaryExprType.MOD;
            case TokenType.less:return BinaryExprType.LT;
            case TokenType.greater: return BinaryExprType.GT;
            case TokenType.lessThanEqual:return BinaryExprType.LEQ;
            case TokenType.greaterThanEqual:return BinaryExprType.GEQ;
            case TokenType.equalTo:return BinaryExprType.EQUAL;
            case TokenType.notEqualTo:return BinaryExprType.NOTEQ;
            case TokenType.and: return BinaryExprType.AND;
            case TokenType.or:return BinaryExprType.OR;
            default: 
                throw new Exception(
                    String.Format("Line {0}:{1}, Expected + - * / % < > <= >= == != && ||, not {2}",
                        exprTokens.Peek().line, exprTokens.Peek().column, exprTokens.Peek().lexeme
                    )
                );
        }
    }

    private int getOperatorPrecedence(Token token) {
        switch(token.type) {
            case TokenType.not:
            case TokenType.minusNegation: 
                return 1;
            case TokenType.multiply:
            case TokenType.divide:
            case TokenType.modus:
                return 2;
            case TokenType.minusSubtraction:
            case TokenType.plus: 
                return 3;
            case TokenType.less:
            case TokenType.greater: 
            case TokenType.lessThanEqual:
            case TokenType.greaterThanEqual:
                return 4;
            case TokenType.equalTo:
            case TokenType.notEqualTo:
                return 5;
            case TokenType.and:
                return 6; 
            case TokenType.or:
                return 7;
            default:
                throw new Exception(
                    String.Format("Line {0}:{1}, {2} is not an operator so preceduence can't be determined",
                        token.line, token.column, token.lexeme
                    )
                );
        }
    }

    private Token consume(TokenType currTokenType) {
        Token expectedToken = exprTokens.Peek();
        if (expectedToken.type == currTokenType) {
            return exprTokens.Dequeue();
        } else {
            throw new Exception(String.Format("Line {0}:{1}, The lexeme {2} does not match with the expected token {3}", 
                expectedToken.line.ToString(), expectedToken.column.ToString(), currTokenType.ToString(), expectedToken.type.ToString()
            ));
        }
    }
}