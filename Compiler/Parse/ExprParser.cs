using CompilerProj.Tokens;

namespace CompilerProj.Parse;

/*
function shuntingYard(infix):
    outputQueue = empty queue
    operatorStack = empty stack
    
    for each token in infix:
        if token is a number:
            create AST node for token
            add AST node to outputQueue
        else if token is an operator:
            while (operatorStack is not empty) AND
                  ((precedence of token < precedence of operatorStack.top()) OR
                  (precedence of token = precedence of operatorStack.top() AND
                   token is left associative)) AND
                  (operatorStack.top() is not left parenthesis):
                add operatorStack.pop() to outputQueue
            push token onto operatorStack
        else if token is left parenthesis:
            push token onto operatorStack
        else if token is right parenthesis:
            while operatorStack.top() is not left parenthesis:
                add operatorStack.pop() to outputQueue
            operatorStack.pop() // Discard the left parenthesis
    while operatorStack is not empty:
        add operatorStack.pop() to outputQueue
    return outputQueue
*/

public class ExprParser {
    private Queue<Token> exprTokens;

    public ExprParser(Queue<Token> exprTokens) {
        this.exprTokens = exprTokens;
    }

    public void parseByShuntingYard() {
        Stack<ExprAST> operandStack = new Stack<ExprAST>();
        Stack<Token> operatorStack = new Stack<Token>();

        while (exprTokens.Count > 0) {
            Token currToken = exprTokens.Dequeue();
            if (isStartOfOperand(currToken)) {

            } else {

            }
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
        switch(token.type) {
            case TokenType.plus:
                return true;
            default:
                return false;
        }
    }
}