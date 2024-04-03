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

    [Test]
    public void test_3np1() {
        string filePath = "../../../TypecheckTesting/ValidTests/3np1.prgm";
        ensureNoTypecheckErrors(filePath);
    }

    [Test]
    public void test_Ack() {
        string filePath = "../../../TypecheckTesting/ValidTests/Ack.prgm";
        ensureNoTypecheckErrors(filePath);
    }

    [Test]
    public void test_binsearch() {
        string filePath = "../../../TypecheckTesting/ValidTests/binsearch.prgm";
        ensureNoTypecheckErrors(filePath);
    }

    [Test]
    public void test_fib() {
        string filePath = "../../../TypecheckTesting/ValidTests/fib.prgm";
        ensureNoTypecheckErrors(filePath);
    }

    [Test]
    public void test_gc() {
        string filePath = "../../../TypecheckTesting/ValidTests/gc.prgm";
        ensureNoTypecheckErrors(filePath);
    }

    [Test]
    public void test_loop() {
        string filePath = "../../../TypecheckTesting/ValidTests/loop.prgm";
        ensureNoTypecheckErrors(filePath);
    }

    [Test]
    public void test_primes() {
        string filePath = "../../../TypecheckTesting/ValidTests/primes.prgm";
        ensureNoTypecheckErrors(filePath);
    }

    [Test]
    public void test_sieve() {
        string filePath = "../../../TypecheckTesting/ValidTests/sieve.prgm";
        ensureNoTypecheckErrors(filePath);
    }

    [Test]
    public void test_simplesearch() {
        string filePath = "../../../TypecheckTesting/ValidTests/binsearch.prgm";
        ensureNoTypecheckErrors(filePath);
    }
}