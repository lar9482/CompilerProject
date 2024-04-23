using CompilerProj.Context;
using CompilerProj.Visitors;

/*
 * This pass will record the symbols of the top level declarations.
 * This includes:
 *      Single variable declarations
 *      Multi variable declarations, with multiple expressions or a function call
 *      Array declarations.
 *      Multi-dimensional array declarations
 *      Functions
 */
public sealed class TypecheckerP1 : TypeChecker {
    public TypecheckerP1() { 
    }

    public override void visit(ProgramAST program) { 
        context.push();

        foreach(DeclAST decl in program.declarations) {
            decl.accept(this);
        }

        foreach(FunctionAST function in program.functions) {
            function.accept(this);
        }

        // Future system call support.
        addIO();
        addSysCallSupport();
        program.scope = context.pop();
    }

    public override void visit(VarDeclAST varDecl) { 
        initializeVarDecl(varDecl);
    }

    public override void visit(MultiVarDeclAST multiVarDecl) { 
        initializeMultiVarDecl(multiVarDecl);
    }

    public override void visit(MultiVarDeclCallAST multiVarDeclCall) {
        initializeMultiVarDeclCall(multiVarDeclCall);
    }

    public override void visit(ArrayDeclAST array) { 
        initializeArrayDecl(array);
    }

    public override void visit(MultiDimArrayDeclAST multiDimArray) { 
        initializeMultiDimArrayDecl(multiDimArray);
    }

    //TODO: Implement this later.
    public override void visit(ArrayDeclCallAST arrayCall) {
        throw new NotImplementedException();
    }

    public override void visit(MultiDimArrayDeclCallAST multiDimArrayCall) {
        throw new NotImplementedException();
    }

    public override void visit(FunctionAST function) { 
        initializeFunction(function);
    }
    
    private void addIO() {
        // Reads a string to standard output
        context.put("print", new SymbolFunction(
            "print",
            new SimpleType[] {
                new ArrayType<PrimitiveType>(new IntType())
            },
            new SimpleType[] {}
        ));

        // Reads a string to standard output, followed by a newline and flush()aybe
        context.put("println", new SymbolFunction(
            "println",
            new SimpleType[] {
                new ArrayType<PrimitiveType>(new IntType())
            },
            new SimpleType[] {}
        ));

        //Read from standard input until a newline
        context.put("readln", new SymbolFunction(   
            "readln",
            new SimpleType[] {},
            new SimpleType[] {
                new ArrayType<PrimitiveType>(new IntType())
            }
        ));

        //Read a single character from standard input
        //Return -1 if the end of input has been reached.
        context.put("getchar", new SymbolFunction(
            "getchar",
            new SimpleType[] {},
            new SimpleType[] {
                new IntType()
            }
        ));


        // Test for end of file on standard input
        context.put("eof", new SymbolFunction(
            "eof",
            new SimpleType[] {},
            new SimpleType[] {
                new BoolType()
            }
        ));
    }

    private void addSysCallSupport() {

        // If "str" contains a sequence of ASCII characters that correctly represent
        // an integer constant n, return (n, true). Otherwise return (0, false).
        context.put("parseInt", new SymbolFunction(
            "parseInt",
            new SimpleType[] {
                new ArrayType<PrimitiveType>(new IntType())
            },
            new SimpleType[] {
                new IntType(), new BoolType()
            }
        ));

        // Return a sequence of ASCII characters representing the
        // integer n.
        context.put("unparseInt", new SymbolFunction(
            "unparseInt",
            new SimpleType[] {
                new IntType()
            },
            new SimpleType[] {
                new ArrayType<PrimitiveType>(new IntType())
            }
        ));

        //Given a single dimensional int array, the length is returned.
        context.put("lengthInt", new SymbolFunction(
            "lengthInt",
            new SimpleType[] {
                new ArrayType<PrimitiveType>(new IntType())
            },
            new SimpleType[] {
                new IntType()
            }
        ));

        //Given a single dimensional bool array, the length is returned.
        context.put("lengthBool", new SymbolFunction(
            "lengthBool",
            new SimpleType[] {
                new ArrayType<PrimitiveType>(new BoolType())
            },
            new SimpleType[] {
                new IntType()
            }
        ));

        //Ensure that the passed in boolean is true.
        context.put("assert", new SymbolFunction(
            "assert",
            new SimpleType[] {
                new BoolType()
            },
            new SimpleType[] { }
        ));
    }
}