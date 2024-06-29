using CompilerProj;

namespace CompilerTests;

/*
 * This testing suite to simply utilize 'PrintVisitor' to pretty log the generated lowered IR from the 
 * all of the program files.
 */
public class PrintAndSaveLIR {

    private string extractFileName(string filePath) {
        int startIndex = filePath.LastIndexOf('/') + 1;
        int endIndex = filePath.IndexOf(".prgm");
        
        return filePath.Substring(startIndex, endIndex - startIndex);
    }

    private void printAndSaveLIR(string inputFilePath) {
        LIRCompUnit loweredIR = Compiler.lowerIR(inputFilePath);
        PrintVisitor printVisitor = new PrintVisitor();
        printVisitor.visit(loweredIR);

        string fileOutput = String.Format("../../../PrintLIROutput/{0}.log", extractFileName(inputFilePath));
        File.WriteAllLines(fileOutput, printVisitor.printContent.ToArray());
    }

    [Test]
    public void funcCall_multiReturn() {
        string filePath = "../../../ProgramFiles/funcCall_multiReturn.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void funcCall_singleReturn1() {
        string filePath = "../../../ProgramFiles/funcCall_singleReturn1.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void funcCall_singleReturn2() {
        string filePath = "../../../ProgramFiles/funcCall_singleReturn2.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void funcCall_singleReturn3() {
        string filePath = "../../../ProgramFiles/funcCall_singleReturn3.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void exprInteger() {
        string filePath = "../../../ProgramFiles/exprInteger.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void exprBool() {
        string filePath = "../../../ProgramFiles/exprBool.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void arrayDecl_noInitialValues() {
        string filePath = "../../../ProgramFiles/arrayDecl_noInitialValues.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void arrayDecl_noInitialValues_thenAssigned() {
        string filePath = "../../../ProgramFiles/arrayDecl_noInitialValues_thenAssigned.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void arrayDecl_hasInitialValues() {
        string filePath = "../../../ProgramFiles/arrayDecl_hasInitialValues.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void arrayDecl_outOfBounds_Positive() {
        string filePath = "../../../ProgramFiles/arrayDecl_outOfBounds_Positive.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void arrayDecl_outOfBounds_Negative() {
        string filePath = "../../../ProgramFiles/arrayDecl_outOfBounds_Negative.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void arrayDeclCall() {
        string filePath = "../../../ProgramFiles/arrayDeclCall.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void whileLoop_10Iterations() {
        string filePath = "../../../ProgramFiles/whileLoop_10Iterations.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void whileLoop_0Iterations() {
        string filePath = "../../../ProgramFiles/whileLoop_0Iterations.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void forLoopSimple() {
        string filePath = "../../../ProgramFiles/forLoopSimple.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void forLoop_DeclsInit() {
        string filePath = "../../../ProgramFiles/forLoop_DeclsInit.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void forLoop_NonDeclsInit() {
        string filePath = "../../../ProgramFiles/forLoop_NonDeclsInit.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void ifStmt() {
        string filePath = "../../../ProgramFiles/ifStmt.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void if_elseif_stmts() {
        string filePath = "../../../ProgramFiles/if_elseif_stmts.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void if_else_stmt() {
        string filePath = "../../../ProgramFiles/if_else_stmt.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void if_elseif_else_stmts() {
        string filePath = "../../../ProgramFiles/if_elseif_else_stmts.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void multiDimArrayDecl_noInitVals() {
        string filePath = "../../../ProgramFiles/multiDimArrayDecl_noInitVals.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void multiDimArrayDecl_noInitVals_thenAssign() {
        string filePath = "../../../ProgramFiles/multiDimArrayDecl_noInitVals_thenAssign.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void multiDimArrayDecl_initVals() {
        string filePath = "../../../ProgramFiles/multiDimArrayDecl_initVals.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void multiDimArrayDeclCall() {
        string filePath = "../../../ProgramFiles/multiDimArrayDeclCall.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void printlnHelloWorld() {
        string filePath = "../../../ProgramFiles/printlnHelloWorld.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void printHelloWorld() {
        string filePath = "../../../ProgramFiles/printHelloWorld.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void assertPass() {
        string filePath = "../../../ProgramFiles/assertPass.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void assertFail() {
        string filePath = "../../../ProgramFiles/assertFail.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void unparseInt() {
        string filePath = "../../../ProgramFiles/unparseInt.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void parseIntAndUnparseInt() {
        string filePath = "../../../ProgramFiles/parseIntAndUnparseInt.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void mutation() {
        string filePath = "../../../ProgramFiles/mutation.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void factorial() {
        string filePath = "../../../EndToEndTests/factorial.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void Binsearch() {
        string filePath = "../../../EndToEndTests/binsearch.prgm";
        printAndSaveLIR(filePath);
    }

    [Test] 
    public void Collatz() {
        string filePath = "../../../EndToEndTests/collatz.prgm";
        printAndSaveLIR(filePath);
    }  

    [Test]
    public void Sort() {
        string filePath = "../../../EndToEndTests/sort.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void SimpleLoop() {
        string filePath = "../../../EndToEndTests/loop.prgm";
        printAndSaveLIR(filePath);
    }

    [Test]
    public void Primes() {
        string filePath = "../../../EndToEndTests/primes.prgm";
        printAndSaveLIR(filePath);
    }
}