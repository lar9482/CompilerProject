using CompilerProj.AST;

internal enum BinaryExprType {
    OR, 
    AND, 
    EQUAL, 
    NOTEQ,
    LT, 
    LEQ, 
    GEQ, 
    GT, 
    ADD, 
    SUB, 
    MULT, 
    DIV,
    MOD
}

internal enum UnaryExprType {
    NEGATE,
    NOT,
}