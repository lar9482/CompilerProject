using CompilerProj;
using System.IO;

namespace CompilerTests;

public class PrintLIR {

    private string extractFileName(string filePath) {
        int startIndex = filePath.LastIndexOf('/') + 1;
        int endIndex = filePath.IndexOf(".prgm");
        
        return filePath.Substring(startIndex, endIndex - startIndex);
    }

    private void printLIR(string inputFilePath, int[] args) {
        LIRCompUnit loweredIR = Compiler.lowerIR(inputFilePath);
        PrintVisitor printVisitor = new PrintVisitor();
        printVisitor.visit(loweredIR);

        string fileOutput = String.Format("../../../PrintLIROutput/{0}.log", extractFileName(inputFilePath));
        File.WriteAllLines(fileOutput, printVisitor.printContent.ToArray());
    }

    [Test]
    public void funcCall_multiReturn() {
        string filePath = "../../../ProgramFiles/funcCall_multiReturn.prgm";
        int[] args = new int[] {
            2, 1
        };
        printLIR(filePath, args);
    }

    [Test]
    public void funcCall_singleReturn1() {
        string filePath = "../../../ProgramFiles/funcCall_singleReturn1.prgm";
        int[] args = new int[] { 10 };
        printLIR(filePath, args);
    }

    [Test]
    public void funcCall_singleReturn2() {
        string filePath = "../../../ProgramFiles/funcCall_singleReturn2.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void funcCall_singleReturn3() {
        string filePath = "../../../ProgramFiles/funcCall_singleReturn3.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void exprInteger() {
        string filePath = "../../../ProgramFiles/exprInteger.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void exprBool() {
        string filePath = "../../../ProgramFiles/exprBool.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void arrayDecl_noInitialValues() {
        string filePath = "../../../ProgramFiles/arrayDecl_noInitialValues.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void arrayDecl_noInitialValues_thenAssigned() {
        string filePath = "../../../ProgramFiles/arrayDecl_noInitialValues_thenAssigned.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void arrayDecl_hasInitialValues() {
        string filePath = "../../../ProgramFiles/arrayDecl_hasInitialValues.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void arrayDecl_outOfBounds_Positive() {
        string filePath = "../../../ProgramFiles/arrayDecl_outOfBounds_Positive.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void arrayDecl_outOfBounds_Negative() {
        string filePath = "../../../ProgramFiles/arrayDecl_outOfBounds_Negative.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void arrayDeclCall() {
        string filePath = "../../../ProgramFiles/arrayDeclCall.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void whileLoop_10Iterations() {
        string filePath = "../../../ProgramFiles/whileLoop_10Iterations.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void whileLoop_0Iterations() {
        string filePath = "../../../ProgramFiles/whileLoop_0Iterations.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void forLoopSimple() {
        string filePath = "../../../ProgramFiles/forLoopSimple.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void forLoop_DeclsInit() {
        string filePath = "../../../ProgramFiles/forLoop_DeclsInit.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void forLoop_NonDeclsInit() {
        string filePath = "../../../ProgramFiles/forLoop_NonDeclsInit.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void ifStmt() {
        string filePath = "../../../ProgramFiles/ifStmt.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void if_elseif_stmts() {
        string filePath = "../../../ProgramFiles/if_elseif_stmts.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void if_else_stmt() {
        string filePath = "../../../ProgramFiles/if_else_stmt.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void if_elseif_else_stmts() {
        string filePath = "../../../ProgramFiles/if_elseif_else_stmts.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void multiDimArrayDecl_noInitVals() {
        string filePath = "../../../ProgramFiles/multiDimArrayDecl_noInitVals.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void multiDimArrayDecl_noInitVals_thenAssign() {
        string filePath = "../../../ProgramFiles/multiDimArrayDecl_noInitVals_thenAssign.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void multiDimArrayDecl_initVals() {
        string filePath = "../../../ProgramFiles/multiDimArrayDecl_initVals.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void multiDimArrayDeclCall() {
        string filePath = "../../../ProgramFiles/multiDimArrayDeclCall.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void printlnHelloWorld() {
        string filePath = "../../../ProgramFiles/printlnHelloWorld.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void printHelloWorld() {
        string filePath = "../../../ProgramFiles/printHelloWorld.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void assertPass() {
        string filePath = "../../../ProgramFiles/assertPass.prgm";
        int[] args = new int[] { };

        printLIR(filePath, args);
    }

    [Test]
    public void assertFail() {
        string filePath = "../../../ProgramFiles/assertFail.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void unparseInt() {
        string filePath = "../../../ProgramFiles/unparseInt.prgm";
        int[] args = new int[] { };

        printLIR(filePath, args);
    }

    [Test]
    public void parseIntAndUnparseInt() {
        string filePath = "../../../ProgramFiles/parseIntAndUnparseInt.prgm";
        int[] args = new int[] { };

        printLIR(filePath, args);
    }

    [Test]
    public void mutation() {
        string filePath = "../../../ProgramFiles/mutation.prgm";
        int[] args = new int[] { };
        printLIR(filePath, args);
    }

    [Test]
    public void factorial() {
        string filePath = "../../../EndToEndTests/factorial.prgm";
        int[] args = new int[] {
            5
        };

        printLIR(filePath, args);
    }

    [Test]
    public void Binsearch() {
        string filePath = "../../../EndToEndTests/binsearch.prgm";
        int[] args = new int[] {};
        printLIR(filePath, args);
    }

    [Test] 
    public void Collatz() {
        string filePath = "../../../EndToEndTests/collatz.prgm";
        int[] args = new int[] {};
        printLIR(filePath, args);
    }  

    [Test]
    public void Sort() {
        string filePath = "../../../EndToEndTests/sort.prgm";
        int[] args = new int[] {};
        printLIR(filePath, args);
    }

    [Test]
    public void SimpleLoop() {
        string filePath = "../../../EndToEndTests/loop.prgm";
        int[] args = new int[] {};

        printLIR(filePath, args);
    }

    [Test]
    public void Primes() {
        string filePath = "../../../EndToEndTests/primes.prgm";
        int[] args = new int[] {};
        printLIR(filePath, args);
    }
}