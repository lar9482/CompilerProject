using CompilerProj.AST;

public enum BinaryExprType {
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

public enum UnaryExprType {
    NEGATE,
    NOT,
}