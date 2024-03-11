using System.Data.Common;
using CompilerProj.AST;
using CompilerProj.Tokens;
using CompilerProj.Types;

namespace CompilerProj.Parse;

/*
 * A simple recursive descent parser with building out the Abstract Syntax Tree
 */
internal sealed class Parser {

    private Queue<Token> tokenQueue;

    private List<FuncDeclAST> topLvl_FuncDecls;
    private List<VarDeclAST> topLvl_VarDecls;
    private List<MultiVarDeclAST> topLvl_MultiVarDecls;
    private List<ArrayAST> topLvl_ArrayDecls;
    private List<MultiDimArrayAST> topLvl_MultiDimArrayDecls;

    internal Parser(Queue<Token> tokenQueue) {
        this.tokenQueue = tokenQueue;

        this.topLvl_FuncDecls = new List<FuncDeclAST>();

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
    internal ProgramAST parseProgram() {
        while (tokenQueue.Peek().type != TokenType.EOF) {
            Token currToken = tokenQueue.Peek();
            switch(currToken.type) {
                case TokenType.identifier:
                    topLvl_FuncDecls.Add(parseFunctionDeclaration());
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
     * ⟨global declaration⟩ ::= ‘:global’ ⟨identifierDecl⟩ ⟨declaration⟩
     */
    private void parseGlobalDeclaration() {
        consume(TokenType.global);
        Tuple<Token, PrimitiveType> identifierAndType = parseIdentifierDeclaration();
        parseDeclaration(
            identifierAndType.Item1,
            identifierAndType.Item2,
            topLvl_VarDecls,
            topLvl_MultiVarDecls,
            topLvl_ArrayDecls,
            topLvl_MultiDimArrayDecls
        );
    }

    /*
     * ⟨declaration⟩ ::= ⟨varDecl ⟩
     *   | ⟨multiVarDecl ⟩
     *   | ⟨identifierDecl⟩ ⟨arrayDeclaration⟩
     */
    private void parseDeclaration(
        Token firstIdentifier,
        PrimitiveType firstType,
        List<VarDeclAST> varDecls,
        List<MultiVarDeclAST> multiVarDecls,
        List<ArrayAST> arrayDecls,
        List<MultiDimArrayAST> multiDimArrayDecls
    ) {
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
            case TokenType.semicolon:
                VarDeclAST varDecl = parseVarDecl(firstIdentifier, firstType);
                varDecls.Add(varDecl);
                break;
            case TokenType.comma:
                MultiVarDeclAST multiVarDecl = parseMultiVarDecls(
                    firstIdentifier, 
                    firstType
                );
                multiVarDecls.Add(multiVarDecl);
                break;
            case TokenType.startBracket:
                parseArrayDeclaration(
                    firstIdentifier, firstType,
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
     * ⟨identifierDecl⟩ ::= ⟨identifier ⟩ <IdentifierType>
     */
    private Tuple<Token, PrimitiveType> parseIdentifierDeclaration() {

        Token identifierToken = consume(TokenType.identifier);
        PrimitiveType primitiveType = parseIdentifierType();

        return Tuple.Create<Token, PrimitiveType>(identifierToken, primitiveType);
    }

    /*
     * <IdentifierType> ::= ‘:’ ⟨primitive type⟩
     */
    private PrimitiveType parseIdentifierType() {
        consume(TokenType.colon);
        PrimitiveType primitiveType = parsePrimitiveType();

        return primitiveType;
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
    internal Dictionary<string, PrimitiveType> parseIdentifierDeclList() {
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

    /*  ⟨singleOrMulti Array Expr ⟩ ::= ‘=’ ⟨arrayInitial ⟩ ‘;’
     *  | ‘[’ ‘]’ ‘=’ ‘{’ ⟨multiDimArrayInitial ⟩ ‘}’ ‘;’
     */
    private void parse_SingleOrMulti_Array_Expr(
        Token identifierToken, PrimitiveType primitiveType,
        List<ArrayAST> arrayDecls,
        List<MultiDimArrayAST> multiDimArrayDecls
    ) {
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
                consume(TokenType.assign);
                ExprAST[] exprs = parseArrayInitial();
                consume(TokenType.semicolon);

                ArrayAST array = new ArrayAST(
                    identifierToken.lexeme,
                    exprs.Length,
                    primitiveType,
                    exprs,
                    identifierToken.line,
                    identifierToken.column
                );
                arrayDecls.Add(array);
                break;
            case TokenType.startBracket:
                consume(TokenType.startBracket);
                consume(TokenType.endBracket);
                consume(TokenType.assign);
                consume(TokenType.startCurly);
                List<ExprAST[]> arrayOfExprs = parseMultiDimArrayInitial();
                verifyMultiDimArrayColumnSize(arrayOfExprs);
                consume(TokenType.endCurly);
                consume(TokenType.semicolon);
                MultiDimArrayAST multiDimArray = new MultiDimArrayAST(
                    identifierToken.lexeme,
                    arrayOfExprs.Count,
                    arrayOfExprs[0].Length,
                    primitiveType,
                    arrayOfExprs.ToArray(),
                    identifierToken.line,
                    identifierToken.column
                );
                multiDimArrayDecls.Add(multiDimArray);
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

    private void verifyMultiDimArrayColumnSize(List<ExprAST[]> arrayOfExprs) {
        int expectedColumnSize = arrayOfExprs[0].Length;
        for (int i = 0; i < arrayOfExprs.Count; i++) {
            if (arrayOfExprs[i].Length != expectedColumnSize) {
                int lineNumber = arrayOfExprs[i][0].lineNumber;
                int columnNumber = arrayOfExprs[i][0].lineNumber;
                throw new Exception(
                    String.Format("Line {0}:{1}, The length of the sub-array doesn't match with {3}", 
                        lineNumber.ToString(), 
                        columnNumber.ToString(),
                        expectedColumnSize
                    )
                );
            }
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
     * ⟨arrayInitial ⟩ ::= ‘{’ <ExprList> ‘}’
     */
    private ExprAST[] parseArrayInitial() {
        consume(TokenType.startCurly);
        List<ExprAST> exprs = parseExprList();
        consume(TokenType.endCurly);

        return exprs.ToArray<ExprAST>();
    }

    /*
     * ⟨multiDimArrayInitial ⟩ ::= ⟨arrayInitial ⟩ ‘,’ ⟨multiDimArrayInitial ⟩
     * | ⟨arrayInitial ⟩
     */
    private List<ExprAST[]> parseMultiDimArrayInitial() {
        List<ExprAST[]> arrayInitials = new List<ExprAST[]>();
        arrayInitials.Add(parseArrayInitial());

        if (tokenQueue.Peek().type != TokenType.comma) {
            return arrayInitials;
        }

        consume(TokenType.comma);
        List<ExprAST[]> nextInitialValues = parseMultiDimArrayInitial();

        return arrayInitials.Concat(nextInitialValues).ToList();
    }

    /*
     * ⟨functionDeclaration⟩ ::= ⟨identifier ⟩ ‘(’ ⟨paramsOptional ⟩ ‘)’ ⟨returnTypesOptional ⟩ ⟨Block ⟩
     */
    private FuncDeclAST parseFunctionDeclaration() {
        Token functionNameToken = consume(TokenType.identifier);
        consume(TokenType.startParen);
        List<ParameterAST> parameters = parseParams();
        consume(TokenType.endParen);

        List<LangType> returnTypes = parseReturnTypes();

        BlockAST block = parseBlock();

        return new FuncDeclAST(
            functionNameToken.lexeme,
            parameters,
            returnTypes,
            block,
            functionNameToken.line,
            functionNameToken.column
        );
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
     * ⟨returnTypes⟩ ::= ‘:’ ⟨Type⟩ ⟨returnTypeList⟩
     * | EPSILON
     */
    private List<LangType> parseReturnTypes() {
        List<LangType> types = new List<LangType>();
        if (tokenQueue.Peek().type != TokenType.colon) {
            return types;
        }

        consume(TokenType.colon);
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
     * <Block> ::= ‘{’ <statements> ‘}’
     */
    private BlockAST parseBlock() {
        Token startCurlyToken = consume(TokenType.startCurly);
        BlockAST blockContent = parseStatements(
            startCurlyToken.line, startCurlyToken.column
        );
        consume(TokenType.endCurly);

        return blockContent;
    }

    private BlockAST parseStatements(int line, int column) {
        List<VarDeclAST> varDecls = new List<VarDeclAST>();
        List<MultiVarDeclAST> multiVarDecls = new List<MultiVarDeclAST>();
        List<ArrayAST> arrayDecls = new List<ArrayAST>();
        List<MultiDimArrayAST> multiDimArrayDecls = new List<MultiDimArrayAST>();
        List<StmtAST> statements = new List<StmtAST>();

        parseStatement(
            varDecls,
            multiVarDecls,
            arrayDecls,
            multiDimArrayDecls,
            statements
        );

        return new BlockAST(
            varDecls,
            multiVarDecls,
            arrayDecls,
            multiDimArrayDecls,
            statements,
            line, column
        );
    }

    /*
     * ⟨statements⟩ ::= ⟨DeclarationOrAssignment⟩
     * | ⟨Conditional ⟩
     * | ⟨WhileLoop⟩
     */
    private void parseStatement(
        List<VarDeclAST> varDecls,
        List<MultiVarDeclAST> multiVarDecls,
        List<ArrayAST> arrayDecls,
        List<MultiDimArrayAST> multiDimArrayDecls,
        List<StmtAST> statements
    ) {
        switch(tokenQueue.Peek().type) {
            case TokenType.identifier:
                parseDeclarationOrAssignment(
                    varDecls,
                    multiVarDecls,
                    arrayDecls,
                    multiDimArrayDecls,
                    statements
                );
                break;
            case TokenType.reserved_if:
                ConditionalAST conditional = parseConditional();
                statements.Add(conditional);
                break;
            case TokenType.reserved_while:
                WhileLoopAST whileLoop = parseWhileLoop();
                statements.Add(whileLoop);
                break;
            case TokenType.reserved_return:
                ReturnAST returnStmt = parseReturn();
                statements.Add(returnStmt);
                break;
            default:
                return;
        }

        parseStatement(
            varDecls,
            multiVarDecls,
            arrayDecls,
            multiDimArrayDecls,
            statements
        );
    }
    /*
     * <DeclarationOrAssignment> ::= <identifier> ‘:’ <primitiveType> <declaration>
     * | <identifier> <assignment>
     */
    private void parseDeclarationOrAssignment(
        List<VarDeclAST> varDecls,
        List<MultiVarDeclAST> multiVarDecls,
        List<ArrayAST> arrayDecls,
        List<MultiDimArrayAST> multiDimArrayDecls,
        List<StmtAST> assignStmts
    ) {
        Token firstIdentifer = consume(TokenType.identifier);
        switch(tokenQueue.Peek().type) {
            case TokenType.colon:
                PrimitiveType firstType = parseIdentifierType();
                parseDeclaration(
                    firstIdentifer,
                    firstType,
                    varDecls,
                    multiVarDecls,
                    arrayDecls,
                    multiDimArrayDecls
                );
                break;
            case TokenType.assign:
            case TokenType.comma:
            case TokenType.startBracket:
                assignStmts.Add(parseAssignment(firstIdentifer));
                break;
        } 
    }

    /*
     * ⟨assignment⟩ ::= ⟨assign⟩
     * | ⟨multiAssign⟩
     * | ⟨arrayAssign⟩
     * | ⟨multiDimArrayAssign⟩
     */
    private StmtAST parseAssignment(Token firstIdentifier) {
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
                return parseAssign(firstIdentifier);
            case TokenType.comma:
                return parseMultiAssign(firstIdentifier);
            case TokenType.startBracket:
                consume(TokenType.startBracket);
                ExprAST firstAccess = parseExpr();
                consume(TokenType.endBracket);
                if (tokenQueue.Peek().type != TokenType.startBracket) {
                    return parseArrayAssign(firstIdentifier, firstAccess);
                } else {
                    return parseMultiDimArrayAssign(firstIdentifier, firstAccess);
                }
            default:
                throw new Exception(
                    String.Format(
                        "Line {0}:{1}, Expected = , [ but not {2}", 
                        tokenQueue.Peek().line.ToString(), 
                        tokenQueue.Peek().column.ToString(),
                        tokenQueue.Peek().lexeme
                    )
                );
        }
    }

    /*
     * ⟨assign⟩ ::= ‘=’ ⟨Expr ⟩ `;'
     */
    private AssignAST parseAssign(Token identifier) {
        consume(TokenType.assign);
        ExprAST expr = parseExpr();
        consume(TokenType.semicolon);

        return new AssignAST(
            new VarAccessAST(identifier.lexeme, identifier.line, identifier.column),
            expr,
            identifier.line, 
            identifier.column
        );
    }

    /*
    <multiAssign> ::= ‘,’ ⟨identifierList⟩ ‘=’ ⟨Expr ⟩ ‘,’ ⟨ExprList⟩ ‘;’
    */
    private MultiAssignAST parseMultiAssign(Token firstIdentifier) {
        List<Token> variableNames = new List<Token>();
        variableNames.Add(firstIdentifier);
        consume(TokenType.comma);

        variableNames = variableNames.Concat(parseIdentifierList()).ToList<Token>();

        consume(TokenType.assign);

        List<ExprAST> exprs = new List<ExprAST>();
        exprs.Add(parseExpr());

        consume(TokenType.comma);
        exprs = exprs.Concat(parseExprList()).ToList<ExprAST>();

        consume(TokenType.semicolon);

        Dictionary<VarAccessAST, ExprAST> assignments = new Dictionary<VarAccessAST, ExprAST>();
        for (int index = 0; index < variableNames.Count; index++) {
            assignments.Add(
                new VarAccessAST(
                    variableNames[index].lexeme, 
                    variableNames[index].line, 
                    variableNames[index].column
                ),
                exprs[index]
            );
        }
        
        return new MultiAssignAST(
            assignments,
            firstIdentifier.line, 
            firstIdentifier.column
        );
    }

     /* ⟨identifierList⟩ ::= ⟨identifier ⟩ ‘,’ ⟨identifierList⟩
      * | ⟨identifier ⟩
      */
    private List<Token> parseIdentifierList() {
        List<Token> identifiers = new List<Token>();
        Token identifier = consume(TokenType.identifier);
        identifiers.Add(identifier);

        if (tokenQueue.Peek().type != TokenType.comma) {
            return identifiers;
        }
        consume(TokenType.comma);

        List<Token> nextIdentifiers = parseIdentifierList();

        return identifiers.Concat(nextIdentifiers).ToList<Token>();
    }

    /*
     * ⟨ExprList⟩ ::= ⟨Expr ⟩ ‘,’ ⟨ExprList⟩
     * | ⟨Expr ⟩
     */
    private List<ExprAST> parseExprList() {
        List<ExprAST> exprs = new List<ExprAST>();
        exprs.Add(parseExpr());

        if (tokenQueue.Peek().type != TokenType.comma) {
            return exprs;
        }

        consume(TokenType.comma);
        
        List<ExprAST> nextExprs = parseExprList();

        return exprs.Concat(nextExprs).ToList<ExprAST>();
    }

    /*
     * ⟨arrayAssign⟩ ::= ‘[’ ⟨Expr ⟩ ‘]’ ‘=’ ⟨Expr ⟩ ‘;’
     */
    private ArrayAssignAST parseArrayAssign(Token identifier, ExprAST access) {

        consume(TokenType.assign);
        ExprAST value = parseExpr();
        consume(TokenType.semicolon);

        return new ArrayAssignAST(
            new ArrayAccessAST(
                identifier.lexeme, 
                access,
                identifier.line,
                identifier.column
            ),
            value,
            identifier.line,
            identifier.column
        );
    }

    /*
     * ⟨multiDimArrayAssign⟩ ::= ‘[’ ⟨Expr ⟩ ‘]’ ‘[’ ⟨Expr ⟩ ‘]’ ‘=’ ⟨Expr ⟩ ‘;’
     */
    private MultiDimArrayAssignAST parseMultiDimArrayAssign(Token identifier, ExprAST firstAccess) {
        consume(TokenType.startBracket);
        ExprAST secondAccess = parseExpr();
        consume(TokenType.endBracket);
        consume(TokenType.assign);
        ExprAST value = parseExpr();
        consume(TokenType.semicolon);

        return new MultiDimArrayAssignAST(
            new MultiDimArrayAccessAST(
                identifier.lexeme,
                firstAccess,
                secondAccess,
                identifier.line,
                identifier.column
            ),
            value,
            identifier.line,
            identifier.column
        );
    }
    
    /*
     * ⟨Conditional ⟩ ::= ‘if’ ‘(’ ⟨Expr ⟩ ‘)’ ⟨block ⟩ ⟨elseIfConditional ⟩ ⟨elseConditional ⟩
     */
    private ConditionalAST parseConditional() {
        ExprAST ifCondition;
        BlockAST ifBlock;
        Dictionary<ExprAST, BlockAST>? elseIfConditionalBlocks = null;
        BlockAST? elseBlock = null;

        Token ifToken = consume(TokenType.reserved_if);
        consume(TokenType.startParen);
        ifCondition = parseExpr();
        consume(TokenType.endParen);
        ifBlock = parseBlock();

        if (tokenQueue.Peek().type != TokenType.reserved_else) {
            return new ConditionalAST(
                ifCondition,
                ifBlock,
                elseIfConditionalBlocks,
                elseBlock,
                ifToken.line,
                ifToken.column
            );
        }

        elseIfConditionalBlocks = new Dictionary<ExprAST, BlockAST>();
        bool seenElseBlock = false;

        while (tokenQueue.Peek().type == TokenType.reserved_else) {
            if (seenElseBlock) {
                throw new Exception(String.Format(
                    "Line {0}:{1}, no 'else if' or 'else' conditionals can exist after the 1st 'else' conditional",
                    tokenQueue.Peek().line.ToString(),
                    tokenQueue.Peek().column.ToString()
                ));
            }

            consume(TokenType.reserved_else);
            switch(tokenQueue.Peek().type) {
                case TokenType.reserved_if:
                    Tuple<ExprAST, BlockAST> elseIfConditional = parseElseIfConditional();
                    elseIfConditionalBlocks.Add(
                        elseIfConditional.Item1,
                        elseIfConditional.Item2
                    );
                    break;
                case TokenType.startCurly:
                    seenElseBlock = true;
                    elseBlock = parseBlock();
                    break;
                default:
                    throw new Exception(
                        String.Format(
                            "Line {0}:{1}, Expected 'if' or '{' ,but not {2}", 
                            tokenQueue.Peek().line.ToString(), 
                            tokenQueue.Peek().column.ToString(),
                            tokenQueue.Peek().lexeme
                        )
                    );
            }
        }

        return new ConditionalAST(
            ifCondition,
            ifBlock,
            elseIfConditionalBlocks,
            elseBlock,
            ifToken.line,
            ifToken.column
        );
    }

    /*
     * ⟨elseIfConditional ⟩ ::= ‘else’ ‘if’ ‘(’ ⟨Expr ⟩ ‘)’ ⟨block ⟩
     */
    private Tuple<ExprAST, BlockAST> parseElseIfConditional() {
        consume(TokenType.reserved_if);
        consume(TokenType.startParen);
        ExprAST condition = parseExpr();
        consume(TokenType.endParen);
        BlockAST block = parseBlock();

        return Tuple.Create<ExprAST, BlockAST>(condition, block);
    }

    /*
     * ⟨whileLoop⟩ ::= ‘while’ ‘(’ ⟨Expr ⟩ ‘)’ ⟨block ⟩
     */
    private WhileLoopAST parseWhileLoop() {
        Token whileToken = consume(TokenType.reserved_while);
        consume(TokenType.startParen);
        ExprAST whileCondition = parseExpr();
        consume(TokenType.endParen);

        BlockAST whileBlock = parseBlock();

        return new WhileLoopAST(
            whileCondition,
            whileBlock,
            whileToken.line,
            whileToken.column
        );
    }

    /*
     * ⟨return⟩ ::= ‘return’ ⟨ExprList⟩? ‘;’
     */
    private ReturnAST parseReturn() {
        Token returnToken = consume(TokenType.reserved_return);
        List<ExprAST>? returnValues = null;

        switch(tokenQueue.Peek().type) {
            case TokenType.minus:
            case TokenType.minusNegation:
            case TokenType.not:
            case TokenType.startParen:
            case TokenType.identifier:
            case TokenType.number:
            case TokenType.reserved_true:
            case TokenType.reserved_false:
                returnValues = parseExprList();
                break;
        }

        consume(TokenType.semicolon);

        return new ReturnAST(
            returnValues,
            returnToken.line,
            returnToken.column
        );
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