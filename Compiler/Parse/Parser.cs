using System.Collections.Concurrent;
using CompilerProj.AST;
using CompilerProj.Tokens;
using CompilerProj.Types;

namespace CompilerProj.Parse;

public class Parser {

    private Queue<Token> tokenQueue;

    private List<VarDeclAST> topLvl_VarDecls;
    private List<MultiVarDeclAST> topLvl_MultiVarDecls;
    private List<ArrayAST> topLvl_ArrayDecls;
    private List<MultiDimArrayAST> topLvl_MultiDimArrayDecls;

    public Parser(Queue<Token> tokenQueue) {
        this.tokenQueue = tokenQueue;

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
    public void parseProgram() {
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
                            "Line {0}:{1}, expected an identifier or :global", 
                            currToken.line.ToString(), 
                            currToken.column.ToString()
                        )
                    );
            }
        }
    }

    /*
     * ⟨global declaration⟩ ::= ‘:global’ ⟨identifierDecl⟩ ⟨declaration⟩
     */
    private void parseGlobalDeclaration() {
        consume(TokenType.global);
        Tuple<Token, PrimitiveType> firstIdentifierType = parseIdentifierDeclaration();

        /*
         * ⟨declaration⟩ ::= ⟨varDecl ⟩
         *   | ⟨multiVarDecl ⟩
         *   | ⟨arrayDecl ⟩
         *   | ⟨multiDimArray⟩
         */
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:
            case TokenType.semicolon:
                VarDeclAST varDecl = parseVarDecl(firstIdentifierType.Item1, firstIdentifierType.Item2);
                topLvl_VarDecls.Add(varDecl);
                break;
            case TokenType.comma:
                MultiVarDeclAST multiVarDecls = parseMultiVarDecls(
                    firstIdentifierType.Item1, 
                    firstIdentifierType.Item2
                );
                topLvl_MultiVarDecls.Add(multiVarDecls);
                break;
            case TokenType.startBracket:
            default:
                break;
        }
    }

    /*
     * ⟨identifierDecl⟩ ::= ⟨identifier ⟩ ‘:’ ⟨primitive type⟩
     */
    private Tuple<Token, PrimitiveType> parseIdentifierDeclaration() {

        Token identifierToken = consume(TokenType.identifier);
        consume(TokenType.colon);
        PrimitiveType primitiveType;

        switch (tokenQueue.Peek().type) {
            case TokenType.reserved_int:
                consume(TokenType.reserved_int);
                primitiveType = new IntType();
                break;
            case TokenType.reserved_bool:
                consume(TokenType.reserved_bool);
                primitiveType = new BoolType();
                break;
            default:
                throw new Exception(
                    String.Format(
                        "Line {0}:{1}, expected int or bool", 
                        tokenQueue.Peek().line.ToString(), 
                        tokenQueue.Peek().column.ToString()
                    )
                );
        }

        return Tuple.Create<Token, PrimitiveType>(identifierToken, primitiveType);
    }

    /*
     *  ⟨varDecl ⟩ ::= ‘=’ ⟨Expr ⟩ ‘;’
     *   | ‘;’
     */
    private VarDeclAST parseVarDecl(Token identifierToken, PrimitiveType primitiveType) {
        switch(tokenQueue.Peek().type) {
            case TokenType.assign:                
                //Parse expressions later
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
                        "Line {0}:{1}, Expected a ; or =", 
                        tokenQueue.Peek().line.ToString(), 
                        tokenQueue.Peek().column.ToString()
                    )
                );
        }
    }

    /*
     * ⟨multiVarDecl ⟩ ::= ‘,’ ⟨multipleDecls⟩ ⟨optional multiple value⟩
     */
    private MultiVarDeclAST parseMultiVarDecls(Token firstIdentifierToken, PrimitiveType firstType) {
        consume(TokenType.comma);

        Dictionary<string, PrimitiveType> identifierTypeMap = new Dictionary<string, PrimitiveType>();
        identifierTypeMap.Add(firstIdentifierToken.lexeme, firstType);

        Dictionary<string, PrimitiveType> nextIdentifierTypeMap = parseMultpleDecls();
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
     *   ⟨multipleDecls⟩ ::= ⟨identifierDecl ⟩ ‘,’ ⟨multipleDecls⟩
     *   | ⟨identifierDecl ⟩
     */
    public Dictionary<string, PrimitiveType> parseMultpleDecls() {
        Dictionary<string, PrimitiveType> identifierTypeMap = new Dictionary<string, PrimitiveType>();
        Tuple<Token, PrimitiveType> identifierTypePair = parseIdentifierDeclaration();

        Token identifierToken = identifierTypePair.Item1;
        PrimitiveType type = identifierTypePair.Item2;

        identifierTypeMap.Add(identifierToken.lexeme, type);

        if (tokenQueue.Peek().type == TokenType.comma) {
            consume(TokenType.comma);
            Dictionary<string, PrimitiveType> nextIdentifierTypeMap = parseMultpleDecls();

            identifierTypeMap = identifierTypeMap.Concat(nextIdentifierTypeMap)
                                .ToDictionary(x => x.Key, x => x.Value);
        }
        return identifierTypeMap; 
    }

    /**
     *  ⟨optional multiple value⟩ ::= ‘=’ ⟨Expr ⟩ ‘,’ ⟨multipleExprs⟩ ‘;’
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

    private void parseFunctionDeclaration() {

    }

    private Token consume(TokenType currTokenType) {
        Token expectedToken = tokenQueue.Peek();
        if (expectedToken.type == currTokenType) {
            return tokenQueue.Dequeue();
        } else {
            throw new Exception(String.Format("Line {0}:{1} {2} does not match with the expected token {3}", 
                expectedToken.line.ToString(), expectedToken.column.ToString(), currTokenType.ToString(), expectedToken.type.ToString()
            ));
        }
    }
}