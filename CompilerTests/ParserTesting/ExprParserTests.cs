using CompilerProj;
using CompilerProj.Lex;
using CompilerProj.Parse;
using CompilerProj.Tokens;
namespace CompilerTests;

public class ExprParserTests {
    [SetUp]
    public void Setup() {
    }

    private Queue<string> getActualTraversalRecord(string text) {
        Lexer lexer = new Lexer();
        Queue<Token> tokenQueue = lexer.lexProgram(text);
        ExprParser parser = new ExprParser(tokenQueue);
        ExprAST expr = parser.parseByShuntingYard();

        ExprVisitor visitor = new ExprVisitor();
        visitor.visit(expr);

        return visitor.traverseRecord;
    }

    private bool matchingTraversalRecord(Queue<string> expectedRecord, Queue<string> actualRecord) {
        if (expectedRecord.Count != actualRecord.Count) {
            return false;
        }

        string currentToken = expectedRecord.Dequeue();
        while (expectedRecord.Count > 0) {
            if (actualRecord.Peek() != currentToken) {
                return false;
            }

            actualRecord.Dequeue();
            currentToken = expectedRecord.Dequeue();
        }
        return true;
    }

    [Test]
    public void ExprParseTest1() {
        string text = "1 - 2 + 3 - 4";
        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("1");
        expectedRecord.Enqueue("2");
        expectedRecord.Enqueue("-");
        expectedRecord.Enqueue("3");
        expectedRecord.Enqueue("+");
        expectedRecord.Enqueue("4");
        expectedRecord.Enqueue("-");

        Queue<string> actualRecord = getActualTraversalRecord(text);
        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test] 
    public void ExprParseTest2() {
        string text = "(1 - 2) + (3 * 4 / 5)";
        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("1");
        expectedRecord.Enqueue("2");
        expectedRecord.Enqueue("-");
        expectedRecord.Enqueue("3");
        expectedRecord.Enqueue("4");
        expectedRecord.Enqueue("*");
        expectedRecord.Enqueue("5");
        expectedRecord.Enqueue("/");
        expectedRecord.Enqueue("+");

        Queue<string> actualRecord = getActualTraversalRecord(text);
        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void ExprParseTest3() {
        string text = "1 + (2 * 3)";
        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("1");
        expectedRecord.Enqueue("2");
        expectedRecord.Enqueue("3");
        expectedRecord.Enqueue("*");
        expectedRecord.Enqueue("+");
        Queue<string> actualRecord = getActualTraversalRecord(text);
        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void ExprParseTest4() {
        string text = "1 + 2 % 3";
        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("1");
        expectedRecord.Enqueue("2");
        expectedRecord.Enqueue("3");
        expectedRecord.Enqueue("%");
        expectedRecord.Enqueue("+");
        Queue<string> actualRecord = getActualTraversalRecord(text);
        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void ExprParseTest5() {
        string text = "-1 + -(2 % 3)";
        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("1");
        expectedRecord.Enqueue("-");
        expectedRecord.Enqueue("2");
        expectedRecord.Enqueue("3");
        expectedRecord.Enqueue("%");
        expectedRecord.Enqueue("-");
        expectedRecord.Enqueue("+");
        Queue<string> actualRecord = getActualTraversalRecord(text);
        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void ExprParseTest6() {
        string text = "true || false && true";

        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("True");
        expectedRecord.Enqueue("False");
        expectedRecord.Enqueue("True");
        expectedRecord.Enqueue("&&");
        expectedRecord.Enqueue("||");
        Queue<string> actualRecord = getActualTraversalRecord(text);
        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void ExprParseTest7() {
        string text = "exprVar1 + array1[55 + 55] + array2[0][-101 - (5 * 3) - (10 / 2)]";
        Queue<string> expectedRecord = new Queue<string>();

        expectedRecord.Enqueue("exprVar1");

        expectedRecord.Enqueue("array1");
        //Index calculation for array1
        expectedRecord.Enqueue("55");
        expectedRecord.Enqueue("55");
        expectedRecord.Enqueue("+");
        //Combining exprVar1 and array2 with the +
        expectedRecord.Enqueue("+");


        expectedRecord.Enqueue("array2");
        //First index
        expectedRecord.Enqueue("0"); 
        //Second index
        expectedRecord.Enqueue("101");
        expectedRecord.Enqueue("-");
        expectedRecord.Enqueue("5");
        expectedRecord.Enqueue("3");
        expectedRecord.Enqueue("*");
        expectedRecord.Enqueue("-");
        expectedRecord.Enqueue("10");
        expectedRecord.Enqueue("2");
        expectedRecord.Enqueue("/");
        expectedRecord.Enqueue("-");

        //Combining everything with the +
        expectedRecord.Enqueue("+");

        Queue<string> actualRecord = getActualTraversalRecord(text);
        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }
}