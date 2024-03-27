using CompilerProj.Context;
using CompilerProj.Visitors;

/*
 * This pass will build symbol tables and annotate the AST with top level declarations
 */
public sealed class TypecheckerP1 : ASTVisitor {

    private Context context;
    private List<string> errorMsgs;

    public TypecheckerP1() { 
        this.context = new Context();
        this.errorMsgs = new List<string>();
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

    public void visit(ProgramAST program) { 
        context.push();

        foreach(DeclAST decl in program.declarations) {
            switch(decl) {
                case VarDeclAST varDecl: varDecl.accept(this); break;
                case MultiVarDeclAST multiVarDecl: multiVarDecl.accept(this); break;
                case ArrayDeclAST arrayDecl: arrayDecl.accept(this); break;
                case MultiDimArrayDeclAST multiDimArrayDecl: multiDimArrayDecl.accept(this); break;
            }
        }

        foreach(FunctionAST function in program.functions) {
            function.accept(this);
        }

        // Future system call support.
        addIO();
        addConv();
        program.scope = context.pop();
    }

    public void visit(VarDeclAST varDecl) { 
        if (context.lookup(varDecl.name) != null) {
            errorMsgs.Add(
                String.Format(
                    "Line {0}:{1}, {2} exists already.", 
                    varDecl.lineNumber, varDecl.columnNumber, varDecl.name
                )
            );
            return;
        }

        SymbolVariable symbolVar = new SymbolVariable(
            varDecl.name, varDecl.declType
        );

        context.put(varDecl.name, symbolVar);
    }

    public void visit(MultiVarDeclAST multiVarDecl) { 
        foreach(KeyValuePair<string, PrimitiveType> nameAndType in multiVarDecl.declTypes) {
            if (context.lookup(nameAndType.Key) != null) {
                errorMsgs.Add(
                    String.Format(
                        "Line {0}:{1}, {2} exists already.", 
                        multiVarDecl.lineNumber, multiVarDecl.columnNumber, nameAndType.Key
                    )
                );
                continue;
            }

            SymbolVariable symbolVar = new SymbolVariable(
                nameAndType.Key,
                nameAndType.Value
            );

            context.put(nameAndType.Key, symbolVar);
        }
    }

    public void visit(MultiVarDeclCallAST multiVarDeclCall) {
        foreach(KeyValuePair<string, PrimitiveType> nameAndType in multiVarDeclCall.declTypes) {
            if (context.lookup(nameAndType.Key) != null) {
                errorMsgs.Add(
                    String.Format(
                        "Line {0}:{1}, {2} exists already.", 
                        multiVarDeclCall.lineNumber, multiVarDeclCall.columnNumber, nameAndType.Key
                    )
                );
                continue;
            }

            SymbolVariable symbolVar = new SymbolVariable(
                nameAndType.Key,
                nameAndType.Value
            );

            context.put(nameAndType.Key, symbolVar);
        }
    }

    public void visit(ArrayDeclAST array) { 
        if (context.lookup(array.name) != null) {
            errorMsgs.Add(
                String.Format(
                    "Line {0}:{1}, {2} exists already.", 
                    array.lineNumber, array.columnNumber, array.name
                )
            );
            return;
        }

        SymbolVariable symbolVar = new SymbolVariable(
            array.name,
            array.declType
        );

        context.put(array.name, symbolVar);
    }

    public void visit(MultiDimArrayDeclAST multiDimArray) { 
        if (context.lookup(multiDimArray.name) != null) {
            errorMsgs.Add(
                String.Format(
                    "Line {0}:{1}, {2} exists already.", 
                    multiDimArray.lineNumber, multiDimArray.columnNumber, multiDimArray.name
                )
            );
            return;
        }

        SymbolVariable symbolVar = new SymbolVariable(
            multiDimArray.name,
            multiDimArray.declType
        );

        context.put(multiDimArray.name, symbolVar);
    }

    public void visit(FunctionAST function) { 
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
    
    //Unused visit procedures
    public void visit(ParameterAST parameter) { }
    public void visit(BlockAST block) { }
    public void visit(ConditionalAST conditional) { }
    public void visit(WhileLoopAST whileLoop) { }
    public void visit(AssignAST assign) { throw new NotImplementedException("This visit is not used"); }
    public void visit(MultiAssignAST multiAssign) { throw new NotImplementedException("This visit is not used"); }
    public void visit(MultiAssignCallAST multiAssignCall) { throw new NotImplementedException("This visit is not used"); }
    public void visit(ArrayAssignAST arrayAssign) { throw new NotImplementedException("This visit is not used"); }
    public void visit(MultiDimArrayAssignAST multiDimArrayAssign) { throw new NotImplementedException("This visit is not used"); }
    public void visit(ReturnAST returnStmt) { throw new NotImplementedException("This visit is not used"); }
    public void visit(FunctionCallAST functionCall) { throw new NotImplementedException("This visit is not used"); }
    public void visit(BinaryExprAST binaryExpr) { throw new NotImplementedException("This visit is not used"); }
    public void visit(UnaryExprAST unaryExpr) { throw new NotImplementedException("This visit is not used"); }
    public void visit(VarAccessAST varAccess) { throw new NotImplementedException("This visit is not used"); }
    public void visit(ArrayAccessAST arrayAccess) { throw new NotImplementedException("This visit is not used"); }
    public void visit(MultiDimArrayAccessAST multiDimArrayAccess) { throw new NotImplementedException("This visit is not used"); }
    public void visit(ProcedureCallAST procedureCall) { throw new NotImplementedException("This visit is not used"); }
    public void visit(IntLiteralAST intLiteral) { throw new NotImplementedException("This visit is not used"); }
    public void visit(BoolLiteralAST boolLiteral) { throw new NotImplementedException("This visit is not used"); }
    public void visit(CharLiteralAST charLiteral) { throw new NotImplementedException("This visit is not used"); }
    public void visit(StrLiteralAST strLiteral) { throw new NotImplementedException("This visit is not used"); }
}