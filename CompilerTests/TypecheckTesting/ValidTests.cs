using CompilerProj;

namespace CompilerTests;

public class ValidTypecheckTests {
    [SetUp]
    public void Setup() {
    }

    private void ensureNoTypecheckErrors(string filePath) {
        try {
            List<string> errorMsgs = Compiler.typecheck(filePath);
            Assert.That(errorMsgs.Count, Is.EqualTo(0));
        } catch (Exception e) {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    public void validTest1() {
        string filePath = "../../../TypecheckTesting/ValidTests/test1.prgm";
        ensureNoTypecheckErrors(filePath);
    }

    [Test]
    public void validTest2() {
        string filePath = "../../../TypecheckTesting/ValidTests/test2.prgm";
        ensureNoTypecheckErrors(filePath);
    }

    [Test]
    public void validTest3() {
        string filePath = "../../../TypecheckTesting/ValidTests/test3.prgm";
        ensureNoTypecheckErrors(filePath);
    }

    [Test]
    public void validTest4() {
        string filePath = "../../../TypecheckTesting/ValidTests/test4.prgm";
        ensureNoTypecheckErrors(filePath);
    }

    [Test]
    public void validTest5() {
        string filePath = "../../../TypecheckTesting/ValidTests/test5.prgm";
        ensureNoTypecheckErrors(filePath);
    }

    [Test]
    public void validTest6() {
        string filePath = "../../../TypecheckTesting/ValidTests/test6.prgm";
        ensureNoTypecheckErrors(filePath);
    }

    [Test]
    public void validTest7() {
        string filePath = "../../../TypecheckTesting/ValidTests/test7.prgm";
        ensureNoTypecheckErrors(filePath);
    }
}