using CompilerProj.Context;
using CompilerProj.Visitors;

/*
 * This pass will record the symbols of the top level declarations.
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
        addConv();
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

    public override void visit(FunctionAST function) { 
        if (context.lookup(function.name) != null) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: {2} exists already.",
                    function.lineNumber,
                    function.columnNumber,
                    function.name
                )
            );
            return;
        }

        List<SimpleType> parameterTypes = new List<SimpleType>();
        foreach(ParameterAST param in function.parameters) {
            parameterTypes.Add(param.type);
        }

        SymbolFunction symbolFunc = new SymbolFunction(
            function.name,
            parameterTypes.ToArray<SimpleType>(),
            function.returnTypes.ToArray<SimpleType>()
        );
        context.put(function.name, symbolFunc);
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

    private void addConv() {

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
    }

    //Unused visit procedures
    public override void visit(ParameterAST parameter) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(BlockAST block) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(ConditionalAST conditional) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(WhileLoopAST whileLoop) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(AssignAST assign) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(MultiAssignAST multiAssign) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(MultiAssignCallAST multiAssignCall) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(ArrayAssignAST arrayAssign) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(MultiDimArrayAssignAST multiDimArrayAssign) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(ReturnAST returnStmt) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(ProcedureCallAST procedureCall) { throw new NotImplementedException("This visit is not used"); }
}