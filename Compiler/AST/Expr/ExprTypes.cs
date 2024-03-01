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
    MODOP
}

public enum UnaryExprType {
    NEGATE,
    NOTOP,
}