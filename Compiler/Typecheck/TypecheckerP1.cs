using System.Reflection.Metadata;
using CompilerProj.Context;
using CompilerProj.Visitors;

/*
 * This pass will build the symbol tables, then annotate the AST with them.
 * Error messages will be reported if symbols are already defined.
 */
public sealed class TypecheckerP1 : ASTVisitor {

    private Context context;
    private List<string> errorMsgs;

    public TypecheckerP1() { 
        this.context = new Context();
        this.errorMsgs = new List<string>();
    }

    private void addIO() {
        context.put("print", new SymbolFunction(
            "print",
            new SimpleType[] {
                new ArrayType<PrimitiveType>(new IntType())
            },
            new SimpleType[] {}
        ));

        context.put("println", new SymbolFunction(
            "println",
            new SimpleType[] {
                new ArrayType<PrimitiveType>(new IntType())
            },
            new SimpleType[] {}
        ));

        context.put("readln", new SymbolFunction(
            "readln",
            new SimpleType[] {},
            new SimpleType[] {
                new ArrayType<PrimitiveType>(new IntType())
            }
        ));

        context.put("getchar", new SymbolFunction(
            "getchar",
            new SimpleType[] {},
            new SimpleType[] {
                new IntType()
            }
        ));

        context.put("eof", new SymbolFunction(
            "eof",
            new SimpleType[] {},
            new SimpleType[] {
                new BoolType()
            }
        ));
    }

    private void addConv() {
        context.put("parseInt", new SymbolFunction(
            "parseInt",
            new SimpleType[] {
                new ArrayType<PrimitiveType>(new IntType())
            },
            new SimpleType[] {
                new IntType(), new BoolType()
            }
        ));

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
            varDecl.name, varDecl.type
        );

        context.put(varDecl.name, symbolVar);
    }

    public void visit(MultiVarDeclAST multiVarDecl) { 
        foreach(KeyValuePair<string, PrimitiveType> nameAndType in multiVarDecl.types) {
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
            array.type
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
            multiDimArray.type
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

        context.push();

        foreach(ParameterAST param in function.parameters) {
            param.accept(this);
        }
        function.block.accept(this);

        SymbolReturn symbolRet = new SymbolReturn(
            function.returnTypes.ToArray<SimpleType>()
        );
        context.put("return", symbolRet);
        
        function.scope = context.pop();
    }

    public void visit(ParameterAST parameter) { 
        if (context.lookup(parameter.name) != null) {
            errorMsgs.Add(
                String.Format(
                    "Line {0}:{1}, {2} exists already.", 
                    parameter.lineNumber, parameter.columnNumber, parameter.name
                )
            );
            return;
        }

        SymbolVariable symbolVar = new SymbolVariable(
            parameter.name,
            parameter.type
        );

        context.put(parameter.name, symbolVar);
    }

    public void visit(BlockAST block) { 
        context.push();

        foreach(DeclAST decl in block.declarations) {
            switch(decl) {
                case VarDeclAST varDecl: varDecl.accept(this); break;
                case MultiVarDeclAST multiVarDecl: multiVarDecl.accept(this); break;
                case ArrayDeclAST arrayDecl: arrayDecl.accept(this); break;
                case MultiDimArrayDeclAST multiDimArrayDecl: multiDimArrayDecl.accept(this); break;
            }
        }

        foreach(StmtAST stmt in block.statements) {
            switch(stmt) {
                case ConditionalAST conditional: conditional.accept(this); break;
                case WhileLoopAST whileLoop: whileLoop.accept(this); break;
            }
        }

        block.scope = context.pop();
    }
    
    public void visit(ConditionalAST conditional) { 
        conditional.ifBlock.accept(this);
        if (conditional.elseIfConditionalBlocks != null) {
            foreach(KeyValuePair<ExprAST, BlockAST> elseIfConditionalBlock in conditional.elseIfConditionalBlocks) {
                elseIfConditionalBlock.Value.accept(this);
            }
        }

        if (conditional.elseBlock != null) {
            conditional.elseBlock.accept(this);
        }
    }

    public void visit(WhileLoopAST whileLoop) { 
        whileLoop.body.accept(this);
    }


    //Unused visit procedures
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