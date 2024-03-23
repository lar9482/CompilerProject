using System.Data.Common;
using CompilerProj.AST;
using CompilerProj.Tokens;
using CompilerProj.Types;

namespace CompilerProj.Parse;

/*
 * A simple recursive descent parser with building out the Abstract Syntax Tree
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
    // private List<VarDeclAST> topLvl_VarDecls;
    // private List<MultiVarDeclAST> topLvl_MultiVarDecls;
    // private List<ArrayDeclAST> topLvl_ArrayDecls;
    // private List<MultiDimArrayDeclAST> topLvl_MultiDimArrayDecls;

    
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
                            "Line {0}:{1}, expected an identifier or :global, not {2}", 
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
     * ⟨global declaration⟩ ::= ‘:global’ ⟨identifierDecl⟩ ⟨declaration⟩
     */
    private void parseGlobalDeclaration() {
        consume(TokenType.global);
        Tuple<Token, PrimitiveType> identifierAndType = parseIdentifierDeclaration();
        parseDeclaration(
            identifierAndType.Item1,
            identifierAndType.Item2,
            topLvl_Declarations
        );
    }

    /*
     * ⟨declaration⟩ ::= ⟨varDecl ⟩
     *   | ⟨multiVarDecl ⟩
     *   | ⟨arrayDeclaration⟩
     */
    private void parseDeclaration(
        Token firstIdentifier,
        PrimitiveType firstType,
        List<DeclAST> declarations
    ) {
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
            case TokenType.semicolon:
                VarDeclAST varDecl = parseVarDecl(firstIdentifier, firstType);
                declarations.Add(varDecl);
                break;
            case TokenType.comma:
                MultiVarDeclAST multiVarDecl = parseMultiVarDecls(
                    firstIdentifier, 
                    firstType
                );
                declarations.Add(multiVarDecl);
                break;
            case TokenType.startBracket:
                parseArrayDeclaration(
                    firstIdentifier, firstType,
                    declarations
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
     *  ⟨optional multiple values⟩ ::= ‘=’ ⟨Expr ⟩ ‘,’ ⟨exprList⟩ ‘;’
     *   | ‘;’
     */
    private List<ExprAST> parseOptionalMultipleValues() {
        List<ExprAST> initialValues = new List<ExprAST>();

        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
                consume(TokenType.assign);
                initialValues.Add(parseExpr());

                consume(TokenType.comma);
                
                List<ExprAST> nextExprs = parseExprList();
                initialValues = initialValues.Concat<ExprAST>(nextExprs).ToList<ExprAST>();

                consume(TokenType.semicolon);
                break;
            case TokenType.semicolon:
                consume(TokenType.semicolon);
                break;
        }

        return initialValues;
    }

     /* ⟨arrayDeclaration⟩ ::= ‘[’ ‘]’ ⟨singleOrMulti_Array_Expr ⟩
      * | ‘[’ ⟨Expr ⟩ ‘]’ ⟨singleOrMulti_Array_Static>
      */
    private void parseArrayDeclaration(
        Token identifierToken, PrimitiveType primitiveType,
        List<DeclAST> declarations
    ) {
        consume(TokenType.startBracket);

        switch(tokenQueue.Peek().type) {
            case TokenType.endBracket:
                consume(TokenType.endBracket);
                parse_SingleOrMulti_Array_Expr(
                    identifierToken, primitiveType,
                    declarations
                );
                break;
            default:
                ExprAST firstExpr = parseExpr();
                consume(TokenType.endBracket);
                parse_SingleOrMulti_Array_Static(
                    identifierToken, primitiveType,
                    declarations,
                    firstExpr
                );
                break;
        }
    }

    /*  ⟨singleOrMulti Array Expr ⟩ ::= ‘=’ ⟨arrayInitial ⟩ ‘;’
     *  | ‘[’ ‘]’ ‘=’ ‘{’ ⟨multiDimArrayInitial ⟩ ‘}’ ‘;’
     */
    private void parse_SingleOrMulti_Array_Expr(
        Token identifierToken, PrimitiveType primitiveType,
        List<DeclAST> declarations
    ) {
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
                consume(TokenType.assign);
                ExprAST[] exprs = parseArrayInitial();
                consume(TokenType.semicolon);

                ArrayDeclAST array = new ArrayDeclAST(
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
                declarations.Add(array);
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
                MultiDimArrayDeclAST multiDimArray = new MultiDimArrayDeclAST(
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
                declarations.Add(multiDimArray);
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
     * | ‘[’ ⟨Expr ⟩ ‘]’ ‘;’
     */
    private void parse_SingleOrMulti_Array_Static(
        Token identifierToken, PrimitiveType primitiveType,
        List<DeclAST> declarations,
        ExprAST firstExpr
    ) {
        switch(tokenQueue.Peek().type) {
            case TokenType.semicolon:
                consume(TokenType.semicolon);

                ArrayDeclAST array = new ArrayDeclAST(
                    identifierToken.lexeme,
                    firstExpr,
                    primitiveType,
                    null,
                    identifierToken.line,
                    identifierToken.column
                );
                declarations.Add(array);
                break;
            case TokenType.startBracket:
                consume(TokenType.startBracket);
                ExprAST secondExpr = parseExpr();
                consume(TokenType.endBracket);
                consume(TokenType.semicolon);

                MultiDimArrayDeclAST multiDimArray = new MultiDimArrayDeclAST(
                    identifierToken.lexeme,
                    firstExpr,
                    secondExpr,
                    primitiveType,
                    null,
                    identifierToken.line,
                    identifierToken.column
                );

                declarations.Add(multiDimArray);
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
        List<DeclAST> declarations = new List<DeclAST>();
        List<StmtAST> statements = new List<StmtAST>();

        parseStatement(
            declarations,
            statements
        );

        return new BlockAST(
            declarations,
            statements,
            line, column
        );
    }

    /*
     * ⟨statements⟩ ::= ⟨DeclarationOrAssignment⟩
     * | ⟨Conditional ⟩
     * | ⟨WhileLoop⟩
     * | <Return >
     */
    private void parseStatement(
        List<DeclAST> declarations,
        List<StmtAST> statements
    ) {
        switch(tokenQueue.Peek().type) {
            case TokenType.identifier:
                parseDeclarationOrAssignment(
                    declarations,
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
            declarations,
            statements
        );
    }
    /*
     * <Declaration_Assignment_ProcedureCall> ::= <identifier> ‘:’ <primitiveType> <declaration>
     * | <identifier> <assignment>
     * | <identifier> <procedureCall>
     */
    private void parseDeclarationOrAssignment(
        List<DeclAST> declarations,
        List<StmtAST> assignStmts
    ) {
        Token firstIdentifer = consume(TokenType.identifier);
        switch(tokenQueue.Peek().type) {
            case TokenType.colon:
                PrimitiveType firstType = parseIdentifierType();
                parseDeclaration(
                    firstIdentifer,
                    firstType,
                    declarations
                );
                break;
            case TokenType.assign:
            case TokenType.comma:
            case TokenType.startBracket:
                assignStmts.Add(parseAssignment(firstIdentifer));
                break;
            case TokenType.startParen:
                assignStmts.Add(parseProcedureCall(firstIdentifer));
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
                return parseMultiAssign_Or_MultiCallAssign(firstIdentifier);
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
     * <multiAssign_Or_MultiCallAssign> ::= ‘,’ ⟨identifierList⟩ ‘=’ ⟨Expr ⟩ ‘,’ ⟨ExprList⟩ ‘;’
     * | ‘,’ ⟨identifierList⟩ ‘=’ <FunctionCall> ‘;’
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

            consume(TokenType.semicolon);
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
            return new MultiAssignCallAST(
                variableAssigns,
                (FunctionCallAST) firstExpr,
                variableAssigns[0].lineNumber,
                variableAssigns[0].columnNumber
            );
        } else {
            throw new Exception(
                String.Format(
                    "Line {0}:{1}: Expected a list of expressions, or just one procedure call",
                    firstIdentifier.line,
                    firstIdentifier.column
                )
            );
        }
    }

    /*
    <multiAssign> ::= ‘,’ ⟨identifierList⟩ ‘=’ ⟨Expr ⟩ ‘,’ ⟨ExprList⟩ ‘;’
    */
    private MultiAssignAST parseMultiAssign(List<Token> variableNames, ExprAST firstExpr) {
        List<ExprAST> exprs = new List<ExprAST>();
        exprs.Add(firstExpr);

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
     * <ProcedureCall > ::= <identifier> ‘(’ ⟨ExprList⟩ ‘)’ `;'
     */
    private ProcedureCallAST parseProcedureCall(Token identifier) {
        consume(TokenType.startParen);

        List<ExprAST> parameters = parseExprList();

        consume(TokenType.endParen);
        consume(TokenType.semicolon);

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