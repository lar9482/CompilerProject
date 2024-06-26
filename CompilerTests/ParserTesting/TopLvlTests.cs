using CompilerProj;

namespace CompilerTests;

/*
 * Testing the parser by doing an incomplete lift from the AST, and excluding the expressions.
 */
public class ParserTests_TopLvl_Lifting {
    [SetUp]
    public void Setup() {

    }

    private Queue<string> getActualTraversalRecord(string programFilePath) {
        ProgramAST program = Compiler.parse(programFilePath);
        TopLvlVisitor visitor = new TopLvlVisitor();
        visitor.visit(program);

        return visitor.traversalRecord;
    }

    private bool matchingTraversalRecord(Queue<string> expectedRecord, Queue<string> actualRecord) {
        if (expectedRecord.Count != actualRecord.Count) {
            return false;
        }

        string currentToken = expectedRecord.Dequeue();
        while (expectedRecord.Count >= 0) {
            if (actualRecord.Peek() != currentToken) {
                return false;
            }

            actualRecord.Dequeue();
            if (expectedRecord.Count == 0) {
                break;
            }
            currentToken = expectedRecord.Dequeue();
        }
        return true;
    }

    [Test]
    public void variableDeclEmpty_Test() {
        string programFilePath = "../../../ParserTesting/TopLvlFiles/variableDeclEmpty.prgm";
        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("variable1: int");
        expectedRecord.Enqueue("variable2: bool");
        expectedRecord.Enqueue("variable7: int");
        expectedRecord.Enqueue("variable8: bool");
        expectedRecord.Enqueue("variable3: int[]");
        expectedRecord.Enqueue("variable4: bool[]");
        expectedRecord.Enqueue("variable5: int[][]");
        expectedRecord.Enqueue("variable6: bool[][]");

        Queue<string> actualRecord = getActualTraversalRecord(programFilePath);
        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void variableDeclExpr_Test() {
        string programFilePath = "../../../ParserTesting/TopLvlFiles/variableDeclExpr.prgm";
        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("variable1: int");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("variable2: bool");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("variable3: int");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("variable4: bool");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("variable5: int[]");
        expectedRecord.Enqueue("ArrayEXPRS");
        expectedRecord.Enqueue("variable6: bool[]");
        expectedRecord.Enqueue("ArrayEXPRS");
        expectedRecord.Enqueue("variable7: int[][]");
        expectedRecord.Enqueue("MultiDimEXPRS");

        Queue<string> actualRecord = getActualTraversalRecord(programFilePath);

        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void funcDecl_NoParams_NoReturns_Test() {
        string programFilePath = "../../../ParserTesting/TopLvlFiles/funcDecl_NoParams_NoReturns.prgm";
        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("function");

        Queue<string> actualRecord = getActualTraversalRecord(programFilePath);

        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void funcDecl_Params_NoReturns_Test() {
        string programFilePath = "../../../ParserTesting/TopLvlFiles/funcDecl_Params_NoReturns.prgm";
        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("function");
        expectedRecord.Enqueue("param1: int");
        expectedRecord.Enqueue("param2: bool");
        expectedRecord.Enqueue("param3: int[]");
        expectedRecord.Enqueue("param4: int[][]");
        expectedRecord.Enqueue("param5: bool[]");
        expectedRecord.Enqueue("param6: bool[][]");

        Queue<string> actualRecord = getActualTraversalRecord(programFilePath);

        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void funcDecl_Params_Returns_Test() {
        string programFilePath = "../../../ParserTesting/TopLvlFiles/funcDecl_Params_Returns.prgm";
        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("function");

        expectedRecord.Enqueue("param1: int");
        expectedRecord.Enqueue("param2: bool");
        expectedRecord.Enqueue("param3: int[]");
        expectedRecord.Enqueue("param4: int[][]");
        expectedRecord.Enqueue("param5: bool[]");
        expectedRecord.Enqueue("param6: bool[][]");

        expectedRecord.Enqueue("int");
        expectedRecord.Enqueue("bool");
        expectedRecord.Enqueue("int[]");
        expectedRecord.Enqueue("int[][]");
        expectedRecord.Enqueue("bool[]");
        expectedRecord.Enqueue("bool[][]");

        Queue<string> actualRecord = getActualTraversalRecord(programFilePath);

        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void stmtAssigns_Test() {
        string programFilePath = "../../../ParserTesting/TopLvlFiles/stmtAssigns.prgm";
        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("function");

        expectedRecord.Enqueue("x");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("x");
        expectedRecord.Enqueue("EXPR");

        expectedRecord.Enqueue("x");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("y");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("z");
        expectedRecord.Enqueue("EXPR");

        expectedRecord.Enqueue("x");
        expectedRecord.Enqueue("y");
        expectedRecord.Enqueue("z");
        expectedRecord.Enqueue("procedure");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("EXPR");

        expectedRecord.Enqueue("a[EXPR]");
        expectedRecord.Enqueue("EXPR");

        expectedRecord.Enqueue("b[EXPR][EXPR]");
        expectedRecord.Enqueue("EXPR");

        Queue<string> actualRecord = getActualTraversalRecord(programFilePath);

        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void stmtDecls_Test() {
        string programFilePath = "../../../ParserTesting/TopLvlFiles/stmtDecls.prgm";
        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("function");
        expectedRecord.Enqueue("variable1: int");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("variable2: bool");
        expectedRecord.Enqueue("EXPR");

        expectedRecord.Enqueue("variable3: int");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("variable4: bool");
        expectedRecord.Enqueue("EXPR");

        expectedRecord.Enqueue("variable5: int[]");
        expectedRecord.Enqueue("ArrayEXPRS");

        expectedRecord.Enqueue("variable6: int[][]");
        expectedRecord.Enqueue("MultiDimEXPRS");

        Queue<string> actualRecord = getActualTraversalRecord(programFilePath);

        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void stmtConditionals_Test() {
        string programFilePath = "../../../ParserTesting/TopLvlFiles/stmtConditionals.prgm";
        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("function");

        expectedRecord.Enqueue("if (EXPR)");
        expectedRecord.Enqueue("x");
        expectedRecord.Enqueue("EXPR");

        expectedRecord.Enqueue("else if (EXPR)");
        expectedRecord.Enqueue("y");
        expectedRecord.Enqueue("EXPR");

        expectedRecord.Enqueue("else if (EXPR)");
        expectedRecord.Enqueue("y");
        expectedRecord.Enqueue("EXPR");

        expectedRecord.Enqueue("if (EXPR)");
        expectedRecord.Enqueue("y");
        expectedRecord.Enqueue("EXPR");

        expectedRecord.Enqueue("if (EXPR)");
        expectedRecord.Enqueue("z");
        expectedRecord.Enqueue("EXPR");

        expectedRecord.Enqueue("else");
        expectedRecord.Enqueue("z");
        expectedRecord.Enqueue("EXPR");

        Queue<string> actualRecord = getActualTraversalRecord(programFilePath);

        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void stmtWhileLoop_Test() {
        string programFilePath = "../../../ParserTesting/TopLvlFiles/stmtWhileLoop.prgm";
        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("function");
        expectedRecord.Enqueue("while (EXPR)");
        expectedRecord.Enqueue("b");
        expectedRecord.Enqueue("EXPR");

        Queue<string> actualRecord = getActualTraversalRecord(programFilePath);

        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void stmtReturn_Test() {
        string programFilePath = "../../../ParserTesting/TopLvlFiles/stmtReturns.prgm";
        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("function");

        expectedRecord.Enqueue("return");
        expectedRecord.Enqueue("EXPR");

        expectedRecord.Enqueue("return");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("EXPR");

        expectedRecord.Enqueue("return");

        Queue<string> actualRecord = getActualTraversalRecord(programFilePath);

        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void arrayDeclCall_Test() {
        string programFilePath = "../../../ParserTesting/TopLvlFiles/arrayDeclCall.prgm";

        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("initializeA");
        expectedRecord.Enqueue("int[]");

        expectedRecord.Enqueue("A: int[]");
        expectedRecord.Enqueue("A[EXPR]");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("A[EXPR]");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("A[EXPR]");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("A[EXPR]");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("A[EXPR]");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("return");
        expectedRecord.Enqueue("EXPR");

        expectedRecord.Enqueue("initializeB");
        expectedRecord.Enqueue("bool[]");

        expectedRecord.Enqueue("B: bool[]");
        expectedRecord.Enqueue("ArrayEXPRS");
        expectedRecord.Enqueue("return");
        expectedRecord.Enqueue("EXPR");

        expectedRecord.Enqueue("main");
        expectedRecord.Enqueue("A: int[]");
        expectedRecord.Enqueue("initializeA");
        expectedRecord.Enqueue("B: bool[]");
        expectedRecord.Enqueue("initializeB");

        Queue<string> actualRecord = getActualTraversalRecord(programFilePath);

        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }

    [Test]
    public void multiDimArrayDeclCall_Test() {
        string programFilePath = "../../../ParserTesting/TopLvlFiles/multiDimArrayDeclCall.prgm";

        Queue<string> expectedRecord = new Queue<string>();
        expectedRecord.Enqueue("initializeA");
        expectedRecord.Enqueue("int[][]");
        expectedRecord.Enqueue("A: int[][]");
        expectedRecord.Enqueue("A[EXPR][EXPR]");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("A[EXPR][EXPR]");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("A[EXPR][EXPR]");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("A[EXPR][EXPR]");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("A[EXPR][EXPR]");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("A[EXPR][EXPR]");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("A[EXPR][EXPR]");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("A[EXPR][EXPR]");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("A[EXPR][EXPR]");
        expectedRecord.Enqueue("EXPR");
        expectedRecord.Enqueue("return");
        expectedRecord.Enqueue("EXPR");


        expectedRecord.Enqueue("initializeB");
        expectedRecord.Enqueue("int[][]");
        expectedRecord.Enqueue("B: int[][]");
        expectedRecord.Enqueue("MultiDimEXPRS");
        expectedRecord.Enqueue("return");
        expectedRecord.Enqueue("EXPR");


        expectedRecord.Enqueue("main");
        expectedRecord.Enqueue("A: int[][]");
        expectedRecord.Enqueue("initializeA");
        expectedRecord.Enqueue("B: int[][]");
        expectedRecord.Enqueue("initializeB");

        Queue<string> actualRecord = getActualTraversalRecord(programFilePath);

        Assert.IsTrue(matchingTraversalRecord(expectedRecord, actualRecord));
    }
}