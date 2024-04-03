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
            Console.WriteLine();
            string errorMsg = e.Message;
            bool isSemanticError = errorMsg.Contains("SemanticError");

            Assert.That(isSemanticError, Is.True);
            return;
        }
        
        List<string> errorMsgs = Compiler.typecheck(filePath);
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
}