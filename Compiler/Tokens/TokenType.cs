namespace CompilerProj.Tokens;

public enum TokenType {
    //Reserved words
    reserved_if, 
    reserved_while, 
    reserved_for,
    reserved_else, 
    reserved_return, 
    reserved_length,
    reserved_int, 
    reserved_bool, 
    reserved_true, 
    reserved_false,

    //Symbols
    comma,
    colon,
    semicolon,
    doubleQuotes,
    startParen,
    endParen,
    startBracket,
    endBracket,
    startCurly,
    endCurly,
    assign,
    plus,
    minus,
    multiply,
    divide,
    modus,
    less,
    greater,
    lessThanEqual,
    greaterThanEqual,
    equalTo,
    notEqualTo,
    and,
    or,
    not,
    
    identifier,
    number,
    global,
    EOF,

    //Context dependent tokens(assigned in the parser phase)
    minusNegation,
    minusSubtraction
}