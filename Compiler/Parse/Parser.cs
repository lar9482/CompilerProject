using CompilerProj.AST;
using CompilerProj.Tokens;
using CompilerProj.Types;

namespace CompilerProj.Parse;

public class Parser {

    private Queue<Token> tokenQueue;

    private List<FuncDecl> topLvl_FuncDecls;
    private List<VarDeclAST> topLvl_VarDecls;
    private List<MultiVarDeclAST> topLvl_MultiVarDecls;
    private List<ArrayAST> topLvl_ArrayDecls;
    private List<MultiDimArrayAST> topLvl_MultiDimArrayDecls;

    public Parser(Queue<Token> tokenQueue) {
        this.tokenQueue = tokenQueue;

        this.topLvl_FuncDecls = new List<FuncDecl>();

        this.topLvl_VarDecls = new List<VarDeclAST>();
        this.topLvl_MultiVarDecls = new List<MultiVarDeclAST>();
        this.topLvl_ArrayDecls = new List<ArrayAST>();
        this.topLvl_MultiDimArrayDecls = new List<MultiDimArrayAST>();
    }

    /*
     *  ⟨definitions⟩ ::= ⟨function declaration⟩ ⟨definitions⟩
     *   | ⟨global declaration⟩ ⟨definitions⟩
     *   | EPSILON
     */
    public ProgramAST parseProgram() {
        while (tokenQueue.Peek().type != TokenType.EOF) {
            Token currToken = tokenQueue.Peek();
            switch(currToken.type) {
                case TokenType.identifier:
                    parseFunctionDeclaration();
                    break;
                case TokenType.global:
                    parseGlobalDeclaration();
                    break;
                default:
                    throw new Exception(
                        String.Format(
                            "Line {0}:{1}, expected an identifier or :global, not {2}", 
                            currToken.line.ToString(), 
                            currToken.column.ToString(),
                            currToken.lexeme
                        )
                    );
            }
        }
        
        ProgramAST program = new ProgramAST(
            topLvl_VarDecls,
            topLvl_MultiVarDecls,
            topLvl_ArrayDecls,
            topLvl_MultiDimArrayDecls,
            topLvl_FuncDecls,
            1,
            1
        );

        return program;
    }

    /*
     * ⟨global declaration⟩ ::= ‘:global’ ⟨declaration⟩
     */
    private void parseGlobalDeclaration() {
        consume(TokenType.global);
        parseDeclaration(
            topLvl_VarDecls,
            topLvl_MultiVarDecls,
            topLvl_ArrayDecls,
            topLvl_MultiDimArrayDecls
        );
    }

    /*
     * ⟨declaration⟩ ::= ⟨identifierDecl⟩ ⟨varDecl ⟩
     *   | ⟨identifierDecl⟩ ⟨multiVarDecl ⟩
     *   | ⟨identifierDecl⟩ ⟨arrayDeclaration⟩
     */
    private void parseDeclaration(
        List<VarDeclAST> varDecls,
        List<MultiVarDeclAST> multiVarDecls,
        List<ArrayAST> arrayDecls,
        List<MultiDimArrayAST> multiDimArrayDecls
    ) {
        Tuple<Token, PrimitiveType> firstIdentifierType = parseIdentifierDeclaration();
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
            case TokenType.semicolon:
                VarDeclAST varDecl = parseVarDecl(firstIdentifierType.Item1, firstIdentifierType.Item2);
                varDecls.Add(varDecl);
                break;
            case TokenType.comma:
                MultiVarDeclAST multiVarDecl = parseMultiVarDecls(
                    firstIdentifierType.Item1, 
                    firstIdentifierType.Item2
                );
                multiVarDecls.Add(multiVarDecl);
                break;
            case TokenType.startBracket:
                parseArrayDeclaration(
                    firstIdentifierType.Item1, firstIdentifierType.Item2,
                    arrayDecls, 
                    multiDimArrayDecls
                );
                break;
            default:
                throw new Exception(
                    String.Format(
                        "Line {0}:{1}, expected an identifer, not {2}", 
                        tokenQueue.Peek().line.ToString(), 
                        tokenQueue.Peek().column.ToString(),
                        tokenQueue.Peek().lexeme
                    )
                );
        }
    }
    /*
     * ⟨identifierDecl⟩ ::= ⟨identifier ⟩ ‘:’ ⟨primitive type⟩
     */
    private Tuple<Token, PrimitiveType> parseIdentifierDeclaration() {

        Token identifierToken = consume(TokenType.identifier);
        consume(TokenType.colon);
        PrimitiveType primitiveType = parsePrimitiveType();

        return Tuple.Create<Token, PrimitiveType>(identifierToken, primitiveType);
    }

    /*
     * ⟨primitive type⟩ ::= "int" | "bool"
     */
    private PrimitiveType parsePrimitiveType() {
        switch (tokenQueue.Peek().type) {
            case TokenType.reserved_int:
                consume(TokenType.reserved_int);
                return new IntType();
            case TokenType.reserved_bool:
                consume(TokenType.reserved_bool);
                return new BoolType();
            default:
                throw new Exception(
                    String.Format(
                        "Line {0}:{1}, expected int or bool, not {2}", 
                        tokenQueue.Peek().line.ToString(), 
                        tokenQueue.Peek().column.ToString(),
                        tokenQueue.Peek().lexeme
                    )
                );
        }
    }

    /*
     *  ⟨varDecl ⟩ ::= ‘=’ ⟨Expr ⟩ ‘;’
     *   | ‘;’
     */
    private VarDeclAST parseVarDecl(Token identifierToken, PrimitiveType primitiveType) {
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
                consume(TokenType.assign);
                ExprAST expression = parseExpr();
                consume(TokenType.semicolon);

                return new VarDeclAST(
                    identifierToken.lexeme,
                    expression,
                    primitiveType,
                    identifierToken.line,
                    identifierToken.column
                );
            case TokenType.semicolon:
                consume(TokenType.semicolon);
                return new VarDeclAST(
                    identifierToken.lexeme,
                    null,
                    primitiveType,
                    identifierToken.line,
                    identifierToken.column
                );
            default:
                throw new Exception(
                    String.Format(
                        "Line {0}:{1}, Expected a ; or =, not {2}", 
                        tokenQueue.Peek().line.ToString(), 
                        tokenQueue.Peek().column.ToString(),
                        tokenQueue.Peek().lexeme
                    )
                );
        }
    }

    /*
     * ⟨multiVarDecl ⟩ ::= ‘,’ ⟨identifierDeclList ⟩ ⟨optional multiple value⟩
     */
    private MultiVarDeclAST parseMultiVarDecls(Token firstIdentifierToken, PrimitiveType firstType) {
        consume(TokenType.comma);

        Dictionary<string, PrimitiveType> identifierTypeMap = new Dictionary<string, PrimitiveType>();
        identifierTypeMap.Add(firstIdentifierToken.lexeme, firstType);

        Dictionary<string, PrimitiveType> nextIdentifierTypeMap = parseIdentifierDeclList();
        identifierTypeMap = identifierTypeMap.Concat(nextIdentifierTypeMap)
                                .ToDictionary(x => x.Key, x => x.Value);

        List<ExprAST> initialValues = parseOptionalMultipleValues();
        List<string> identifiers = new List<string>(identifierTypeMap.Keys);

        Dictionary<string, ExprAST?> identifier_initialValuesMap = new Dictionary<string, ExprAST?>();
        for (int i = 0; i < identifiers.Count; i++) {
            if ((i+1) > initialValues.Count) {
                identifier_initialValuesMap.Add(identifiers[i], null);
            } else {
                identifier_initialValuesMap.Add(identifiers[i], initialValues[i]);
            }
        }

        return new MultiVarDeclAST(
            identifiers,
            identifier_initialValuesMap,
            identifierTypeMap,
            firstIdentifierToken.line,
            firstIdentifierToken.column
        );
    }

    /**
     *   ⟨identifierDeclList⟩ ::= ⟨identifierDecl ⟩ ‘,’ ⟨identifierDeclList ⟩
     *   | ⟨identifierDecl ⟩
     */
    public Dictionary<string, PrimitiveType> parseIdentifierDeclList() {
        Dictionary<string, PrimitiveType> identifierTypeMap = new Dictionary<string, PrimitiveType>();
        Tuple<Token, PrimitiveType> identifierTypePair = parseIdentifierDeclaration();

        Token identifierToken = identifierTypePair.Item1;
        PrimitiveType type = identifierTypePair.Item2;

        identifierTypeMap.Add(identifierToken.lexeme, type);

        if (tokenQueue.Peek().type == TokenType.comma) {
            consume(TokenType.comma);
            Dictionary<string, PrimitiveType> nextIdentifierTypeMap = parseIdentifierDeclList();

            identifierTypeMap = identifierTypeMap.Concat(nextIdentifierTypeMap)
                                .ToDictionary(x => x.Key, x => x.Value);
        }
        return identifierTypeMap; 
    }

    /**
     *  ⟨optional multiple value⟩ ::= ‘=’ ⟨Expr ⟩ ‘,’ ⟨exprList⟩ ‘;’
     *   | ‘;’
     */
    private List<ExprAST> parseOptionalMultipleValues() {
        List<ExprAST> initialValues = new List<ExprAST>();

        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
                consume(TokenType.assign);
                //Parse multiple expressions later.
                break;
            case TokenType.semicolon:
                consume(TokenType.semicolon);
                break;
        }

        return initialValues;
    }

     /* ⟨arrayDeclaration⟩ ::= ‘[’ ‘]’ ⟨singleOrMulti_Array_Expr ⟩
      * | ‘[’ ⟨number ⟩ ‘]’ ⟨singleOrMulti_Array_Static>
      */
    private void parseArrayDeclaration(
        Token identifierToken, PrimitiveType primitiveType,
        List<ArrayAST> arrayDecls,
        List<MultiDimArrayAST> multiDimArrayDecls
    ) {
        consume(TokenType.startBracket);

        switch(tokenQueue.Peek().type) {
            case TokenType.endBracket:
                consume(TokenType.endBracket);
                parse_SingleOrMulti_Array_Expr(
                    identifierToken, primitiveType,
                    arrayDecls, multiDimArrayDecls
                );
                break;
            case TokenType.number:
                Token numberToken = consume(TokenType.number);
                consume(TokenType.endBracket);
                parse_SingleOrMulti_Array_Static(
                    identifierToken, primitiveType,
                    arrayDecls, multiDimArrayDecls,
                    Int32.Parse(numberToken.lexeme)
                );
                break;
            default:
                throw new Exception(
                    String.Format(
                        "Line {0}:{1}, Expected a ] or a number, not {2}", 
                        tokenQueue.Peek().line.ToString(), 
                        tokenQueue.Peek().column.ToString(),
                        tokenQueue.Peek().lexeme
                    )
                );
        }
    }

    /*  ⟨singleOrMulti Array Expr ⟩ ::= ‘=’ ⟨arrayInitial ⟩
     *  | ‘[’ ‘]’ ‘=’ ‘{’ ⟨multiDimArrayInitial ⟩ ‘}’
     */
    private void parse_SingleOrMulti_Array_Expr(
        Token identifierToken, PrimitiveType primitiveType,
        List<ArrayAST> arrayDecls,
        List<MultiDimArrayAST> multiDimArrayDecls
    ) {
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
                consume(TokenType.assign);
                //Parse array initial later. Requires the expression parser.
                break;
            case TokenType.startBracket:
                consume(TokenType.startBracket);
                consume(TokenType.endBracket);
                consume(TokenType.assign);
                consume(TokenType.startCurly);
                //Parse multiDimArrayInitial later. Requires the expression parser.
                break;
            default:
                throw new Exception(
                    String.Format(
                        "Line {0}:{1}, Expected = or [, not {2}", 
                        tokenQueue.Peek().line.ToString(), 
                        tokenQueue.Peek().column.ToString(),
                        tokenQueue.Peek().lexeme
                    )
                );
        }
    }

    /*
     * ⟨singleOrMulti Array Static⟩ ::= ‘;’
     * | ‘[’ ⟨number ⟩ ‘]’ ‘;’
     */
    private void parse_SingleOrMulti_Array_Static(
        Token identifierToken, PrimitiveType primitiveType,
        List<ArrayAST> arrayDecls,
        List<MultiDimArrayAST> multiDimArrayDecls,
        int firstNumber
    ) {
        switch(tokenQueue.Peek().type) {
            case TokenType.semicolon:
                consume(TokenType.semicolon);

                ArrayAST array = new ArrayAST(
                    identifierToken.lexeme,
                    firstNumber,
                    primitiveType,
                    null,
                    identifierToken.line,
                    identifierToken.column
                );
                arrayDecls.Add(array);
                break;
            case TokenType.startBracket:
                consume(TokenType.startBracket);
                Token secondNumberToken = consume(TokenType.number);
                consume(TokenType.endBracket);
                consume(TokenType.semicolon);

                MultiDimArrayAST multiDimArray = new MultiDimArrayAST(
                    identifierToken.lexeme,
                    firstNumber,
                    Int32.Parse(secondNumberToken.lexeme),
                    primitiveType,
                    null,
                    identifierToken.line,
                    identifierToken.column
                );

                multiDimArrayDecls.Add(multiDimArray);
                break;
            default:
                break;
        }
    }

    /*
     * ⟨functionDeclaration⟩ ::= ⟨identifier ⟩ ‘(’ ⟨paramsOptional ⟩ ‘)’ ‘:’ ⟨returnTypesOptional ⟩ ⟨Block ⟩
     */
    private void parseFunctionDeclaration() {
        Token functionNameToken = consume(TokenType.identifier);
        consume(TokenType.startParen);
        List<ParameterAST> parameters = parseParams();
        consume(TokenType.endParen);

        consume(TokenType.colon);
        List<LangType> returnTypes = parseReturnTypes();
        Console.WriteLine();
    }

    /*
     * ⟨params⟩ ::= ⟨identifier ⟩ `:' ⟨Type⟩ ⟨paramsList⟩
     * | EPSILON
     */
    private List<ParameterAST> parseParams() {
        List<ParameterAST> parameters = new List<ParameterAST>();
        if (tokenQueue.Peek().type != TokenType.identifier) {
            return parameters;
        }

        Token parameterNameToken = consume(TokenType.identifier);
        consume(TokenType.colon);
        LangType type = parseType();
        ParameterAST firstParameter = new ParameterAST(
            parameterNameToken.lexeme,
            type,
            parameterNameToken.line,
            parameterNameToken.column
        );

        parameters.Add(firstParameter);
        List<ParameterAST> nextParameters = parseParamsList();

        return parameters.Concat(nextParameters).ToList();
    }

    /*
     * ⟨paramsList⟩ ::= ‘,’ ⟨identifier ⟩ `:' ⟨Type⟩ ⟨paramsList⟩
     * | EPSILON
     */
    private List<ParameterAST> parseParamsList() {
        List<ParameterAST> parameters = new List<ParameterAST>();
        if (tokenQueue.Peek().type != TokenType.comma) {
            return parameters;
        }
        consume(TokenType.comma);

        Token parameterNameToken = consume(TokenType.identifier);
        consume(TokenType.colon);

        LangType type = parseType();
        ParameterAST parameter = new ParameterAST(
            parameterNameToken.lexeme,
            type,
            parameterNameToken.line,
            parameterNameToken.column
        );

        parameters.Add(parameter);

        List<ParameterAST> nextParameters = parseParamsList();

        return parameters.Concat(nextParameters).ToList();
    }

    /*
     * <Type> ::= <primitiveType> <typeArray>
     */
    private LangType parseType() {
        PrimitiveType primitiveType = parsePrimitiveType();
        LangType? arrayType = parseTypeArray(primitiveType);

        return (arrayType == null) ? (primitiveType) : (arrayType);
    }

    /*
     * <typeArray> ::=  `[' `]' <typeMultiDimArray>
     * | `[' `]'
     * | EPSILON
     */

    private LangType? parseTypeArray(PrimitiveType type) {
        if (tokenQueue.Peek().type != TokenType.startBracket) {
            return null;
        }

        consume(TokenType.startBracket);
        consume(TokenType.endBracket);

        return (parseTypeMultiDimArray()) ? (
            new MultiDimArrayType<PrimitiveType>(type)
        ) : (
            new ArrayType<PrimitiveType>(type)
        );
    }

    /*
     * <typeMultiDimArray> ::= `[' `]'
     *| EPSILON
     */
    private bool parseTypeMultiDimArray() {
        if (tokenQueue.Peek().type != TokenType.startBracket) {
            return false;
        }

        consume(TokenType.startBracket);
        consume(TokenType.endBracket);
        return true;
    }

    /*
     * ⟨returnTypes⟩ ::= ⟨Type⟩ ⟨returnTypeList⟩
     * | EPSILON
     */
    private List<LangType> parseReturnTypes() {
        List<LangType> types = new List<LangType>();
        if (tokenQueue.Peek().type != TokenType.reserved_int 
        && tokenQueue.Peek().type != TokenType.reserved_bool
        ) {
            return types;
        }

        LangType type = parseType();
        types.Add(type);

        List<LangType> nextTypes = parseReturnTypeList();

        return types.Concat(nextTypes).ToList();
    }

    /* 
     * ⟨returnTypeList⟩ ::= ‘,’ ⟨Type⟩ ⟨returnTypeList⟩
     * | EPSILON
     */
    private List<LangType> parseReturnTypeList() {
        List<LangType> types = new List<LangType>();

        if (tokenQueue.Peek().type != TokenType.comma) {
            return types;
        }
        
        consume(TokenType.comma);
        LangType type = parseType();
        types.Add(type);

        List<LangType> nextTypes = parseReturnTypeList();

        return types.Concat(nextTypes).ToList();
    }
    /*
     */
    private void parseBlock() {
        consume(TokenType.startCurly);

        consume(TokenType.endCurly);
    }

    private ExprAST parseExpr() {
        ExprParser exprParser = new ExprParser(tokenQueue);
        return exprParser.parseByShuntingYard();
    }
    

    private Token consume(TokenType currTokenType) {
        Token expectedToken = tokenQueue.Peek();
        if (expectedToken.type == currTokenType) {
            return tokenQueue.Dequeue();
        } else {
            throw new Exception(String.Format("Line {0}:{1}, The lexeme {2} does not match with the expected token {3}", 
                expectedToken.line.ToString(), expectedToken.column.ToString(), currTokenType.ToString(), expectedToken.type.ToString()
            ));
        }
    }
}