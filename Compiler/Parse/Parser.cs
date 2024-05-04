using System.Data.Common;
using CompilerProj.AST;
using CompilerProj.Tokens;
using CompilerProj.Types;

namespace CompilerProj.Parse;


//TODO: Use a parser generator sometime, or refactor the grammar? This is easily the most spaghetti module of this project.
/*
 * A recursive descent parser for building out the Abstract Syntax Tree
 * 
 * NOTE: This isn't a pure recursive descent parser, because there are parts where ambiguity is kind of abused.
 *
 * For example, if there is a grammar rule
 * <A> ::= 'b' <B>
 * | 'b' <C>
 *
 * instead of writing the grammar completely context free, you may see something like
 * 
 * parseA() {
 *   Token bToken = consume('b')
 *   switch(tokenQueue.Peek().type) {
 *      case TokenThatIndicatesB:
 *          parseB(bToken)
 *      case TokenThatIndicatesC:
 *          parseC(bToken)
 *   }
 * }
 * 
 * parseB(Token bToken) {
 *   //Consume <B> tokens
 * }
 *
 * parseC(Token bToken) {
 *   //Consume <C> tokens
 * }
 *
 * This case is especially prevalent with declarations, procedure calls, and assignments, because they all start with
 * an identifier token.
 *
 * Also, if the grammar rule has an EPSILON such as
 * <A> :: 'a' 'b'
 *  | EPSILON
 * 
 * you may see something like
 * parseA() {
 *     if (tokenQueue.Peek() != 'a') {
 *          return;
 *     }
 *     consume('a')
 *     consume('b')
 * }
 *
 *

 }
 */
public sealed class Parser {

    private Queue<Token> tokenQueue;

    private List<FunctionAST> topLvl_Functions;
    private List<DeclAST> topLvl_Declarations;
    
    public Parser(Queue<Token> tokenQueue) {
        this.tokenQueue = tokenQueue;

        this.topLvl_Functions = new List<FunctionAST>();
        this.topLvl_Declarations = new List<DeclAST>();
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
                    topLvl_Functions.Add(parseFunctionDeclaration());
                    break;
                case TokenType.global:
                    parseGlobalDeclaration();
                    break;
                default:
                    throw new Exception(
                        String.Format(
                            "{0}:{1} ParseError: expected an identifier or :global, not {2}", 
                            currToken.line.ToString(), 
                            currToken.column.ToString(),
                            currToken.lexeme
                        )
                    );
            }
        }
        
        ProgramAST program = new ProgramAST(
            topLvl_Declarations,
            topLvl_Functions,
            1,
            1
        );

        return program;
    }

    /*
     * ⟨global declaration⟩ ::= ‘:global’ ⟨identifierDecl⟩ ⟨declaration⟩ ‘;’ 
     */
    private void parseGlobalDeclaration() {
        consume(TokenType.global);
        Tuple<Token, PrimitiveType> identifierAndType = parseIdentifierDeclaration();
        topLvl_Declarations.Add(
            parseDeclaration(
                identifierAndType.Item1,
                identifierAndType.Item2
            )
        );

        consume(TokenType.semicolon);
    }

    /*
     * ⟨declaration⟩ ::= ⟨varDecl ⟩
     *   | ⟨multiVarDecl ⟩
     *   | ⟨multiVarDeclCall ⟩
     *   | ⟨arrayDeclaration⟩
     */
    private DeclAST parseDeclaration(
        Token firstIdentifier,
        PrimitiveType firstType
    ) {
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
            case TokenType.semicolon:
                VarDeclAST varDecl = parseVarDecl(firstIdentifier, firstType);
                return varDecl;
            case TokenType.comma:
                DeclAST multiDecl = parseMultiVarDecls_Or_MultiVarDeclCall(
                    firstIdentifier, 
                    firstType
                );
                return multiDecl;
            case TokenType.startBracket:
                return parseArrayDeclaration(
                    firstIdentifier, firstType
                );
            default:
                throw new Exception(
                    String.Format(
                        "{0}:{1} ParseError: expected an identifer, not {2}", 
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
                        "{0}:{1} ParseError: expected int or bool, not {2}", 
                        tokenQueue.Peek().line.ToString(), 
                        tokenQueue.Peek().column.ToString(),
                        tokenQueue.Peek().lexeme
                    )
                );
        }
    }

    /*
     *  ⟨varDecl ⟩ ::= ‘=’ ⟨Expr ⟩
     *   | EPSILON
     */
    private VarDeclAST parseVarDecl(Token identifierToken, PrimitiveType primitiveType) {
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
                consume(TokenType.assign);
                ExprAST expression = parseExpr();

                return new VarDeclAST(
                    identifierToken.lexeme,
                    expression,
                    primitiveType,
                    identifierToken.line,
                    identifierToken.column
                );
            default:
                return new VarDeclAST(
                    identifierToken.lexeme,
                    null,
                    primitiveType,
                    identifierToken.line,
                    identifierToken.column
                );
        }
    }

    /*
     * ⟨multiVarDecl Or multiVarDeclCall⟩ ::= ‘,’ ⟨identifierDeclList ⟩ ⟨optionalMultipleValues_Or_FunctionCall⟩
     */
    private DeclAST parseMultiVarDecls_Or_MultiVarDeclCall(Token firstIdentifierToken, PrimitiveType firstType) {
        consume(TokenType.comma);

        Dictionary<string, PrimitiveType> identifierTypeMap = new Dictionary<string, PrimitiveType>();
        identifierTypeMap.Add(firstIdentifierToken.lexeme, firstType);

        Dictionary<string, PrimitiveType> nextIdentifierTypeMap = parseIdentifierDeclList();
        identifierTypeMap = identifierTypeMap.Concat(nextIdentifierTypeMap)
                                .ToDictionary(x => x.Key, x => x.Value);

        List<ExprAST> initialValues = parseOptionalMultipleValues_Or_FunctionCall();

        // A single expression that is a function call indicates a MultiVarDeclCall.
        if (initialValues.Count == 1 && initialValues[0].GetType() == typeof(FunctionCallAST)) {
            FunctionCallAST functionCall = (FunctionCallAST) initialValues[0];
            return new MultiVarDeclCallAST(
                new List<string>(identifierTypeMap.Keys),
                identifierTypeMap,
                functionCall.functionName,
                functionCall.args,
                firstIdentifierToken.line,
                firstIdentifierToken.column
            );
        }

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
     *  ⟨optional multiple values or function call⟩ ::= ‘=’ ⟨Expr ⟩ <multipleValues>
     *   | ‘=’ <FunctionCallAST>
     *   | EPSILON
     */
    private List<ExprAST> parseOptionalMultipleValues_Or_FunctionCall() {
        List<ExprAST> initialValues = new List<ExprAST>();

        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
                consume(TokenType.assign);
                ExprAST firstExpr = parseExpr();

                if (firstExpr.GetType() == typeof(FunctionCallAST) 
                && tokenQueue.Peek().type==TokenType.semicolon) {
                    initialValues.Add(firstExpr);

                } else if (tokenQueue.Peek().type==TokenType.comma) {
                    initialValues = parseMultipleValues(firstExpr);
                }
                break;
            default:
                break;
        }

        return initialValues;
    }

    /*
     * <multipleValues> ::= ‘,’ ⟨exprList⟩
     */
    private List<ExprAST> parseMultipleValues(ExprAST firstExpr) {
        List<ExprAST> initialValues = new List<ExprAST>();

        initialValues.Add(firstExpr);
        consume(TokenType.comma);
                
        List<ExprAST> nextExprs = parseExprList();
        initialValues = initialValues.Concat<ExprAST>(nextExprs).ToList<ExprAST>();

        return initialValues;
    }

     /* ⟨arrayDeclaration⟩ ::= ‘[’ ‘]’ ⟨singleOrMulti_Array_Expr ⟩
      * | ‘[’ ⟨Expr ⟩ ‘]’ ⟨singleOrMulti_Array_Static>
      */
    private DeclAST parseArrayDeclaration(
        Token identifierToken, PrimitiveType primitiveType
    ) {
        consume(TokenType.startBracket);

        switch(tokenQueue.Peek().type) {
            case TokenType.endBracket:
                consume(TokenType.endBracket);
                return parse_SingleOrMulti_Array_Expr(
                    identifierToken, primitiveType
                );
            default:
                ExprAST firstExpr = parseExpr();
                consume(TokenType.endBracket);
                return parse_SingleOrMulti_Array_Static(
                    identifierToken, primitiveType, firstExpr
                );
        }
    }

    /*  ⟨singleOrMulti Array Expr ⟩ ::= ‘=’ ⟨arrayInitial ⟩
     *  | ‘=’ <ProcedureCall>
     *  | ‘[’ ‘]’ ‘=’ ‘{’ ⟨multiDimArrayInitial ⟩ ‘}’
     *  | ‘[’ ‘]’ ‘=’ <ProcedureCall>
     */
    private DeclAST parse_SingleOrMulti_Array_Expr(
        Token identifierToken, PrimitiveType primitiveType
    ) {
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
                consume(TokenType.assign);
                // Indicating the start of a single dim array declared with initial values
                if (tokenQueue.Peek().type == TokenType.startCurly) {
                    ExprAST[] exprs = parseArrayInitial();

                    ArrayDeclAST arrayDecl = new ArrayDeclAST(
                        identifierToken.lexeme,
                        new IntLiteralAST(
                            exprs.Length,
                            identifierToken.line,
                            identifierToken.column
                        ),
                        primitiveType,
                        exprs,
                        identifierToken.line,
                        identifierToken.column
                    );
                    return arrayDecl;
                } // Indicating the start of a single dim array declared with a function call
                else if (tokenQueue.Peek().type == TokenType.identifier) {
                    Token procedureIdentifier = consume(TokenType.identifier);
                    ProcedureCallAST procedureCall = parseProcedureCall(procedureIdentifier);

                    ArrayDeclCallAST arrayCall = new ArrayDeclCallAST(
                        identifierToken.lexeme,
                        new FunctionCallAST(
                            procedureCall.procedureName,
                            procedureCall.args,
                            procedureCall.lineNumber,
                            procedureCall.columnNumber
                        ),
                        primitiveType,
                        identifierToken.line,
                        identifierToken.column
                    );

                    return arrayCall;
                } else {
                    throw new Exception(
                        String.Format(
                            "{0}:{1} ParseError: Expected { or <identifier>, not {2}", 
                            tokenQueue.Peek().line.ToString(), 
                            tokenQueue.Peek().column.ToString(),
                            tokenQueue.Peek().lexeme
                        )
                    );
                }
                
            case TokenType.startBracket:
                consume(TokenType.startBracket);
                consume(TokenType.endBracket);
                consume(TokenType.assign);

                // Indicating the start of a multi dim array declared with initial values
                if (tokenQueue.Peek().type == TokenType.startCurly) {
                    consume(TokenType.startCurly);
                    List<ExprAST[]> arrayOfExprs = parseMultiDimArrayInitial();
                    verifyMultiDimArrayColumnSize(arrayOfExprs);
                    consume(TokenType.endCurly);

                    MultiDimArrayDeclAST multiDimArrayDecl = new MultiDimArrayDeclAST(
                        identifierToken.lexeme,
                        new IntLiteralAST(
                            arrayOfExprs.Count,
                            identifierToken.line,
                            identifierToken.column
                        ),
                        new IntLiteralAST(
                            arrayOfExprs[0].Length,
                            identifierToken.line,
                            identifierToken.column
                        ),
                        primitiveType,
                        arrayOfExprs.ToArray(),
                        identifierToken.line,
                        identifierToken.column
                    );
                    return multiDimArrayDecl;
                } // Indicating the start of a multi dim array declared with a function call.
                else if (tokenQueue.Peek().type == TokenType.identifier) {
                    Token procedureIdentifier = consume(TokenType.identifier);
                    ProcedureCallAST procedureCall = parseProcedureCall(procedureIdentifier);

                    MultiDimArrayDeclCallAST multiDimArrayDeclCall = new MultiDimArrayDeclCallAST(
                        identifierToken.lexeme,
                        new FunctionCallAST(
                            procedureCall.procedureName,
                            procedureCall.args,
                            procedureCall.lineNumber,
                            procedureCall.columnNumber
                        ),
                        primitiveType,
                        identifierToken.line,
                        identifierToken.column
                    );

                    return multiDimArrayDeclCall;
                } else {
                    throw new Exception(
                        String.Format(
                            "{0}:{1} ParseError: Expected { or <identifier>, not {2}", 
                            tokenQueue.Peek().line.ToString(), 
                            tokenQueue.Peek().column.ToString(),
                            tokenQueue.Peek().lexeme
                        )
                    );
                }
            default:
                throw new Exception(
                    String.Format(
                        "{0}:{1} ParseError: Expected [ or =, not {2}", 
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
                    String.Format("{0}:{1} ParseError: The length of the sub-array doesn't match with {3}", 
                        lineNumber.ToString(), 
                        columnNumber.ToString(),
                        expectedColumnSize
                    )
                );
            }
        }
    }
    /*
     * ⟨singleOrMulti Array Static⟩ ::= ‘[’ ⟨Expr ⟩ ‘]’
     * | EPSILON
     */
    private DeclAST parse_SingleOrMulti_Array_Static(
        Token identifierToken, PrimitiveType primitiveType, ExprAST firstExpr
    ) {
        switch(tokenQueue.Peek().type) {
            case TokenType.startBracket:
                consume(TokenType.startBracket);
                ExprAST secondExpr = parseExpr();
                consume(TokenType.endBracket);

                MultiDimArrayDeclAST multiDimArrayDecl = new MultiDimArrayDeclAST(
                    identifierToken.lexeme,
                    firstExpr,
                    secondExpr,
                    primitiveType,
                    null,
                    identifierToken.line,
                    identifierToken.column
                );

                return multiDimArrayDecl;
            default:
                ArrayDeclAST arrayDecl = new ArrayDeclAST(
                    identifierToken.lexeme,
                    firstExpr,
                    primitiveType,
                    null,
                    identifierToken.line,
                    identifierToken.column
                );
                return arrayDecl;
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
    private FunctionAST parseFunctionDeclaration() {
        Token functionNameToken = consume(TokenType.identifier);
        consume(TokenType.startParen);
        List<ParameterAST> parameters = parseParams();
        consume(TokenType.endParen);

        List<SimpleType> returnTypes = parseReturnTypes();

        BlockAST block = parseBlock();

        return new FunctionAST(
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
        SimpleType type = parseType();
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

        SimpleType type = parseType();
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
    private SimpleType parseType() {
        PrimitiveType primitiveType = parsePrimitiveType();
        SimpleType? arrayType = parseTypeArray(primitiveType);

        return (arrayType == null) ? (primitiveType) : (arrayType);
    }

    /*
     * <typeArray> ::=  `[' `]' <typeMultiDimArray>
     * | `[' `]'
     * | EPSILON
     */

    private SimpleType? parseTypeArray(PrimitiveType type) {
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
    private List<SimpleType> parseReturnTypes() {
        List<SimpleType> types = new List<SimpleType>();
        if (tokenQueue.Peek().type != TokenType.colon) {
            return types;
        }

        consume(TokenType.colon);
        SimpleType type = parseType();
        types.Add(type);

        List<SimpleType> nextTypes = parseReturnTypeList();

        return types.Concat(nextTypes).ToList();
    }

    /* 
     * ⟨returnTypeList⟩ ::= ‘,’ ⟨Type⟩ ⟨returnTypeList⟩
     * | EPSILON
     */
    private List<SimpleType> parseReturnTypeList() {
        List<SimpleType> types = new List<SimpleType>();

        if (tokenQueue.Peek().type != TokenType.comma) {
            return types;
        }
        
        consume(TokenType.comma);
        SimpleType type = parseType();
        types.Add(type);

        List<SimpleType> nextTypes = parseReturnTypeList();

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
        List<StmtAST> statements = new List<StmtAST>();

        parseStatement(
            statements
        );

        return new BlockAST(
            statements,
            line, column
        );
    }

    /*
     * ⟨statements⟩ ::= ⟨IdentifierStmt_All⟩ ‘;’
     * | ⟨Conditional ⟩
     * | ⟨WhileLoop⟩
     * | <Return > ‘;’
     */
    private void parseStatement(
        List<StmtAST> statements
    ) {
        switch(tokenQueue.Peek().type) {
            case TokenType.identifier:
                parseIdentifierStmt_All(
                    statements
                );
                consume(TokenType.semicolon);
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
                consume(TokenType.semicolon);
                break;
            case TokenType.endCurly:
                return;
            default:
                throw new Exception(
                    String.Format(
                        "{0}:{1} ParseError: Expected <identifier> <if> <while> <return>, not {2}", 
                        tokenQueue.Peek().line.ToString(), 
                        tokenQueue.Peek().column.ToString(),
                        tokenQueue.Peek().lexeme
                    )
                );
        }

        parseStatement(
            statements
        );
    }
    /*
     * <IdentifierStmt_All> ::= <identifier> ‘:’ <primitiveType> <declaration>
     * | <identifier> <assignOrMutate>
     * | <identifier> <procedureCall>
     * | <identifier> <multiAssign_Or_MultiCallAssign>
     */
    private void parseIdentifierStmt_All(
        List<StmtAST> blockStmts
    ) {
        Token firstIdentifier = consume(TokenType.identifier);
        switch(tokenQueue.Peek().type) {
            case TokenType.colon:
                PrimitiveType firstType = parseIdentifierType();
                blockStmts.Add(
                    parseDeclaration(
                        firstIdentifier,
                        firstType
                    )
                );
                break;
            case TokenType.assign:
            case TokenType.startBracket:
                blockStmts.Add(parseIdent_AssignOrMutation(firstIdentifier));
                break;
            case TokenType.comma:
                blockStmts.Add(parseMultiAssign_Or_MultiCallAssign(firstIdentifier));
                break;
            case TokenType.startParen:
                blockStmts.Add(parseProcedureCall(firstIdentifier));
                break;
            default:
                throw new Exception(
                    String.Format(
                        "{0}:{1} ParseError: Expected : = , [ (, not {2}", 
                        tokenQueue.Peek().line.ToString(), 
                        tokenQueue.Peek().column.ToString(),
                        tokenQueue.Peek().lexeme
                    )
                );
        } 
    }

    /*
     * <ident_AssignOrMutation> ::= ⟨assign⟩ //Returns VarAssignAST
     * | ⟨mutate⟩ // Returns VarMutationAST
     * | ‘[’ ⟨Expr ⟩ ‘]’ ⟨arrayAssignOrMutation⟩
     */
    private StmtAST parseIdent_AssignOrMutation(Token firstIdentifier) {
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
                ExprAST initialVal = parseAssignExpr();
                return new VarAssignAST(
                    new VarAccessAST(
                        firstIdentifier.lexeme,
                        firstIdentifier.line,
                        firstIdentifier.column
                    ),
                    initialVal,
                    firstIdentifier.line,
                    firstIdentifier.column
                );
            case TokenType.plus:
            case TokenType.minus:
            case TokenType.minusNegation:
            case TokenType.minusSubtraction:
                bool increment = parseMutation();
                return new VarMutateAST(
                    new VarAccessAST(
                        firstIdentifier.lexeme,
                        firstIdentifier.line,
                        firstIdentifier.column
                    ),
                    increment,
                    firstIdentifier.line,
                    firstIdentifier.column
                );
            case TokenType.startBracket:
                consume(TokenType.startBracket);
                ExprAST firstIndex = parseExpr();
                consume(TokenType.endBracket);
                return parseArray_AssignOrMutation(
                    firstIdentifier, firstIndex
                );
            default:
                throw new Exception(
                    String.Format(
                        "{0}:{1} ParseError: Expected = + - [, not {2}", 
                        tokenQueue.Peek().line.ToString(), 
                        tokenQueue.Peek().column.ToString(),
                        tokenQueue.Peek().lexeme
                    )
                );
        }
    }

    /*
     * <Array_AssignOrMutation> ::= ⟨assign⟩ //Returns ArrayAssignAST
     * | ⟨mutate⟩ // Returns ArrayMutationAST
     * | ‘[’ ⟨Expr ⟩ ‘]’ ⟨multiDimArray_AssignOrMutation⟩
     */
    private StmtAST parseArray_AssignOrMutation(Token firstIdentifier, ExprAST firstIndex) {
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
                ExprAST initialVal = parseAssignExpr();
                return new ArrayAssignAST(
                    new ArrayAccessAST(
                        firstIdentifier.lexeme,
                        firstIndex,
                        firstIdentifier.line,
                        firstIdentifier.column
                    ),
                    initialVal,
                    firstIdentifier.line,
                    firstIdentifier.column
                );
            case TokenType.plus:
            case TokenType.minus:
            case TokenType.minusNegation:
            case TokenType.minusSubtraction:
                bool increment = parseMutation();
                return new ArrayMutateAST(
                    new ArrayAccessAST(
                        firstIdentifier.lexeme,
                        firstIndex,
                        firstIdentifier.line,
                        firstIdentifier.column
                    ),
                    increment,
                    firstIdentifier.line,
                    firstIdentifier.column
                );
            case TokenType.startBracket:
                consume(TokenType.startBracket);
                ExprAST secondIndex = parseExpr();
                consume(TokenType.endBracket);
                return parseMultiDimArray_AssignOrMutation(
                    firstIdentifier, firstIndex, secondIndex
                );
            default:
                throw new Exception(
                    String.Format(
                        "{0}:{1} ParseError: Expected = + - [, not {2}", 
                        tokenQueue.Peek().line.ToString(), 
                        tokenQueue.Peek().column.ToString(),
                        tokenQueue.Peek().lexeme
                    )
                );
        }
    }

    /*
     * <MultiDimArray_AssignOrMutation> ::= ⟨assign⟩ //Returns MultiDimArrayAssignAST
     * | ⟨mutate⟩ // Returns MultiDimArrayMutationAST
     */
    private StmtAST parseMultiDimArray_AssignOrMutation(
        Token firstIdentifier, ExprAST firstIndex, ExprAST secondIndex
    ) {
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
                ExprAST initialVal = parseAssignExpr();
                return new MultiDimArrayAssignAST(
                    new MultiDimArrayAccessAST(
                        firstIdentifier.lexeme,
                        firstIndex,
                        secondIndex,
                        firstIdentifier.line,
                        firstIdentifier.column
                    ),
                    initialVal,
                    firstIdentifier.line,
                    firstIdentifier.column
                );
            case TokenType.plus:
            case TokenType.minus:
            case TokenType.minusNegation:
            case TokenType.minusSubtraction:
                bool increment = parseMutation();
                return new MultiDimArrayMutateAST(
                    new MultiDimArrayAccessAST(
                        firstIdentifier.lexeme,
                        firstIndex,
                        secondIndex,
                        firstIdentifier.line,
                        firstIdentifier.column
                    ),
                    increment,
                    firstIdentifier.line,
                    firstIdentifier.column
                );
            default:
                throw new Exception(
                    String.Format(
                        "{0}:{1} ParseError: Expected = + -, not {2}", 
                        tokenQueue.Peek().line.ToString(), 
                        tokenQueue.Peek().column.ToString(),
                        tokenQueue.Peek().lexeme
                    )
                );
        }
    }
    /*
     * <AssignExpr> ::= ‘=’ ⟨Expr ⟩
     */
    private ExprAST parseAssignExpr() {
        consume(TokenType.assign);
        ExprAST expr = parseExpr();

        return expr;
    }

    /*
     * <mutation> ::= ++
     * | --
     */
    private bool parseMutation() {
        switch(tokenQueue.Peek().type) {
            case TokenType.plus:
                consume(TokenType.plus);
                consume(TokenType.plus);
                return true;
            case TokenType.minus:
                consume(TokenType.minus);
                consume(TokenType.minus);
                return false;
            case TokenType.minusNegation:
                consume(TokenType.minusNegation);
                consume(TokenType.minusNegation);
                return false;
            case TokenType.minusSubtraction:
                consume(TokenType.minusSubtraction);
                consume(TokenType.minusSubtraction);
                return false;
            default:
                throw new Exception(
                    String.Format(
                        "{0}:{1} ParseError: Expected + or -, not {2}", 
                        tokenQueue.Peek().line.ToString(), 
                        tokenQueue.Peek().column.ToString(),
                        tokenQueue.Peek().lexeme
                    )
                );
        }
    }

    /*
     * <multiAssign_Or_MultiCallAssign> ::= ‘,’ ⟨identifierList⟩ ‘=’ ⟨Expr ⟩ ‘,’ ⟨ExprList⟩
     * | ‘,’ ⟨identifierList⟩ ‘=’ <FunctionCall>
     */
    private StmtAST parseMultiAssign_Or_MultiCallAssign(Token firstIdentifier) {
        List<Token> variableNames = new List<Token>();
        variableNames.Add(firstIdentifier);
        consume(TokenType.comma);

        variableNames = variableNames.Concat(parseIdentifierList()).ToList<Token>();

        consume(TokenType.assign);

        ExprAST firstExpr = parseExpr();

        if (tokenQueue.Peek().type == TokenType.comma) {
            return parseMultiAssign(variableNames, firstExpr);
        } else if (firstExpr.GetType() == typeof(FunctionCallAST)) {
            List<VarAccessAST> variableAssigns = new List<VarAccessAST>();
            foreach (Token variableName in variableNames) {
                variableAssigns.Add(
                    new VarAccessAST(
                        variableName.lexeme,
                        variableName.line,
                        variableName.column
                    )
                );
            }
            
            FunctionCallAST functionCallAST = (FunctionCallAST) firstExpr;
            return new MultiAssignCallAST(
                variableAssigns,
                functionCallAST.functionName,
                functionCallAST.args,
                variableAssigns[0].lineNumber,
                variableAssigns[0].columnNumber
            );
        } else {
            throw new Exception(
                String.Format(
                    "{0}:{1} ParseError: Expected a list of expressions, or just one procedure call",
                    firstIdentifier.line,
                    firstIdentifier.column
                )
            );
        }
    }

    /*
    <multiAssign> ::= ‘,’ ⟨identifierList⟩ ‘=’ ⟨Expr ⟩ ‘,’ ⟨ExprList⟩
    */
    private MultiAssignAST parseMultiAssign(List<Token> variableNames, ExprAST firstExpr) {
        List<ExprAST> exprs = new List<ExprAST>();
        exprs.Add(firstExpr);

        consume(TokenType.comma);
        exprs = exprs.Concat(parseExprList()).ToList<ExprAST>();

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
            variableNames[0].line, 
            variableNames[1].column
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
     * <ProcedureCall > ::= <identifier> ‘(’ ⟨ExprList⟩? ‘)’
     */
    private ProcedureCallAST parseProcedureCall(Token identifier) {
        consume(TokenType.startParen);

        List<ExprAST> parameters;

        //Case when there are no expressions passed in.
        if (tokenQueue.Peek().type == TokenType.endParen) {
            parameters = new List<ExprAST>();
        } else {
            parameters = parseExprList();
        }

        consume(TokenType.endParen);

        return new ProcedureCallAST(
            identifier.lexeme,
            parameters,
            identifier.line,
            identifier.column
        );
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

    // /*
    //  * ⟨multiDimArrayAssign⟩ ::= ‘[’ ⟨Expr ⟩ ‘]’ ‘[’ ⟨Expr ⟩ ‘]’ ‘=’ ⟨Expr ⟩
    //  */
    // private MultiDimArrayAssignAST parseMultiDimArrayAssign(Token identifier, ExprAST firstAccess) {
    //     consume(TokenType.startBracket);
    //     ExprAST secondAccess = parseExpr();
    //     consume(TokenType.endBracket);
    //     consume(TokenType.assign);
    //     ExprAST value = parseExpr();

    //     return new MultiDimArrayAssignAST(
    //         new MultiDimArrayAccessAST(
    //             identifier.lexeme,
    //             firstAccess,
    //             secondAccess,
    //             identifier.line,
    //             identifier.column
    //         ),
    //         value,
    //         identifier.line,
    //         identifier.column
    //     );
    // }
    
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
                    "{0}:{1} ParseError: no 'else if' or 'else' conditionals can exist after the 1st 'else' conditional",
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
                            "{0}:{1} ParseError: Expected 'if' or '{' ,but not {2}", 
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
     * ⟨return⟩ ::= ‘return’ ⟨ExprList⟩?
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
            throw new Exception(String.Format("{0}:{1} ParseError: The lexeme {2} does not match with the expected token {3}", 
                expectedToken.line.ToString(), expectedToken.column.ToString(), currTokenType.ToString(), expectedToken.type.ToString()
            ));
        }
    }
}