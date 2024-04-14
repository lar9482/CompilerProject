using CompilerProj;

namespace CompilerTests;

public class InvalidTypecheckTests {
    [SetUp]
    public void Setup() {
    }

    private void ensureSomeTypecheckErrors(string filePath) {
        try {
            Compiler.typecheck(filePath);
        } catch (Exception e) {
            string errorMsg = e.Message;
            bool isSemanticError = errorMsg.Contains("SemanticError");

            Assert.That(isSemanticError, Is.True);
            return;
        }
        
        Tuple<ProgramAST, List<string>> ASTWithErrors = Compiler.typecheck(filePath);
        List<string> errorMsgs = ASTWithErrors.Item2;
        Assert.That(errorMsgs.Count, Is.GreaterThan(0));
    }

    [Test]
    public void invalidTest1() {
        string filePath = "../../../TypecheckTesting/InvalidTests/test1.prgm";
        ensureSomeTypecheckErrors(filePath);
    }

    [Test]
    public void invalidTest2() {
        string filePath = "../../../TypecheckTesting/InvalidTests/test2.prgm";
        ensureSomeTypecheckErrors(filePath);
    }

    [Test]
    public void invalidTest3() {
        string filePath = "../../../TypecheckTesting/InvalidTests/test3.prgm";
        ensureSomeTypecheckErrors(filePath);
    }

    [Test]
    public void invalidTest4() {
        string filePath = "../../../TypecheckTesting/InvalidTests/test4.prgm";
        ensureSomeTypecheckErrors(filePath);
    }

    [Test]
    public void invalidTest5() {
        string filePath = "../../../TypecheckTesting/InvalidTests/test5.prgm";
        ensureSomeTypecheckErrors(filePath);
    }

    [Test]
    public void invalidTest6() {
        string filePath = "../../../TypecheckTesting/InvalidTests/test6.prgm";
        ensureSomeTypecheckErrors(filePath);
    }

    [Test]
    public void invalidTest7() {
        string filePath = "../../../TypecheckTesting/InvalidTests/test7.prgm";
        ensureSomeTypecheckErrors(filePath);
    }

    [Test]
    public void invalidTest8() {
        string filePath = "../../../TypecheckTesting/InvalidTests/test8.prgm";
        ensureSomeTypecheckErrors(filePath);
    }

    [Test]
    public void invalidTest9() {
        string filePath = "../../../TypecheckTesting/InvalidTests/test9.prgm";
        ensureSomeTypecheckErrors(filePath);
    }
}