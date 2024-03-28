using CompilerProj.Context;
using CompilerProj.Types;
using CompilerProj.Visitors;

/*
 * This pass will typecheck top level declarations.
 */
public sealed class TypecheckerP2 : TypeChecker {

    public override void visit(ProgramAST program) {
        if (program.scope == null) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} Semantic Error: Top level scope was not initialized",
                    program.lineNumber, program.columnNumber
                )
            );
            return;
        } 
        context.push(program.scope);

        foreach(DeclAST decl in program.declarations) {
            decl.accept(this);
        }

        program.scope = context.pop();
    }

    public override void visit(VarDeclAST varDecl) { 
        varDecl.type = new UnitType();

        SymbolVariable varSymbol = (SymbolVariable) lookUpSymbolFromContext(
            varDecl.name, varDecl.lineNumber, varDecl.columnNumber
        );

        if (varDecl.initialValue == null) {
            return;
        }
        varDecl.initialValue.accept(this);
        SimpleType initialValueType = varDecl.initialValue.type;
        SimpleType varDeclType = varSymbol.type;

        if (!sameTypes(initialValueType, varDeclType)) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: The initial value type, {2}, doesn't match with the variable declaration.",
                    varDecl.initialValue.lineNumber,
                    varDecl.initialValue.columnNumber,
                    simpleTypeToString(initialValueType)
                )
            );
        }
    }

    public override void visit(MultiVarDeclAST multiVarDecl) { 
        multiVarDecl.type = new UnitType();

        foreach(string varName in multiVarDecl.names) {
            SymbolVariable varSymbol = (SymbolVariable) lookUpSymbolFromContext(
                varName, multiVarDecl.lineNumber, multiVarDecl.columnNumber
            );

            ExprAST? initialValue = multiVarDecl.initialValues[varName];
            if (initialValue == null) {
                continue;
            }

            initialValue.accept(this);
            SimpleType initialValueType = initialValue.type;

            if (!sameTypes(initialValueType, varSymbol.type)) {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: The initial value type, {2}, doesn't match with the variable declaration.",
                        initialValue.lineNumber,
                        initialValue.columnNumber,
                        simpleTypeToString(initialValueType)
                    )
                );
            }
        }
    }

    public override void visit(MultiVarDeclCallAST multiVarDeclCallAST) {
        multiVarDeclCallAST.type = new UnitType();

        SymbolFunction symbolFunction = (SymbolFunction) lookUpSymbolFromContext(
            multiVarDeclCallAST.functionCall.functionName,
            multiVarDeclCallAST.lineNumber, multiVarDeclCallAST.columnNumber
        );

        for (int i = 0; i < multiVarDeclCallAST.names.Count; i++) {
            string varName = multiVarDeclCallAST.names[i];
            SymbolVariable symbolVar = (SymbolVariable) lookUpSymbolFromContext(
                varName, multiVarDeclCallAST.lineNumber, multiVarDeclCallAST.columnNumber
            );

            SimpleType variableType = symbolVar.type;
            SimpleType returnFuncType = symbolFunction.returnTypes[i];
            if (!sameTypes(variableType , returnFuncType)) {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: {2}'s type doesn't match with the return type {3}",
                        multiVarDeclCallAST.lineNumber,
                        multiVarDeclCallAST.columnNumber,
                        varName, simpleTypeToString(returnFuncType)
                    )
                );
            }
        }

        FunctionCallAST functionCall = multiVarDeclCallAST.functionCall;
        for (int i = 0; i < functionCall.args.Count; i++) {
            ExprAST param = functionCall.args[i];
            param.accept(this);

            SimpleType paramType = param.type;
            SimpleType expectedParamType = symbolFunction.parameterTypes[i];
            
            if (!sameTypes(paramType, expectedParamType)) {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: Param's type {2} doesn't match with the expected type {3}",
                        param.lineNumber, param.columnNumber,
                        simpleTypeToString(paramType), simpleTypeToString(expectedParamType)
                    )
                );
            }
        }
    }

    public override void visit(ArrayDeclAST array) { 
        array.type = new UnitType();

        array.size.accept(this);
        SimpleType arraySizeType = array.size.type;

        if (arraySizeType.TypeTag != "int" && arraySizeType.TypeTag != "bool") {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: The size expression must be an int or a bool",
                    array.size.lineNumber, array.size.columnNumber
                )
            );
        }

        if (array.initialValues == null) {
            return;
        }

        SymbolVariable symbolVar = (SymbolVariable) lookUpSymbolFromContext(
            array.name, array.lineNumber, array.columnNumber
        );

        SimpleType expectedInitValType;
        switch(symbolVar.type) {
            case ArrayType<PrimitiveType> arrayDecl when arrayDecl.baseType.TypeTag=="int": 
                expectedInitValType = new IntType();
                break;
            case ArrayType<PrimitiveType> arrayDecl when arrayDecl.baseType.TypeTag=="bool":
                expectedInitValType = new BoolType();
                break;
            default:
                throw new Exception(
                    String.Format(
                        "{0}:{1} SemanticError: The array type could not be resolved incorrectly"
                    )
                );
        }

        foreach(ExprAST initialValue in array.initialValues) {
            initialValue.accept(this);
            SimpleType actualInitValType = initialValue.type;

            if (!sameTypes(expectedInitValType, actualInitValType)) {
                errorMsgs.Add(
                    String.Format(
                        "{0}:{1} SemanticError: The initial value must be of type {2}",
                        initialValue.lineNumber, initialValue.columnNumber,
                        simpleTypeToString(actualInitValType)
                    )
                );
            }
        }
    }

    public override void visit(MultiDimArrayDeclAST multiDimArray) { 
        multiDimArray.type = new UnitType();

        multiDimArray.rowSize.accept(this);
        multiDimArray.colSize.accept(this);

        SimpleType rowSizeType = multiDimArray.rowSize.type;
        SimpleType colSizeType = multiDimArray.colSize.type;

        if (rowSizeType.TypeTag != "int" && rowSizeType.TypeTag != "bool") {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: The row size expression must be an int or a bool",
                    multiDimArray.rowSize.lineNumber, multiDimArray.rowSize.columnNumber
                )
            );
        }
        if (colSizeType.TypeTag != "int" && colSizeType.TypeTag != "bool") {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: The column size expression must be an int or a bool",
                    multiDimArray.colSize.lineNumber, multiDimArray.colSize.columnNumber
                )
            );
        }

        if (multiDimArray.initialValues == null) {
            return;
        }

        SymbolVariable symbolVar = (SymbolVariable) lookUpSymbolFromContext(
            multiDimArray.name, multiDimArray.lineNumber, multiDimArray.columnNumber
        );

        SimpleType expectedInitValType;
        switch(symbolVar.type) {
            case MultiDimArrayType<PrimitiveType> arrayDecl when arrayDecl.baseType.TypeTag=="int": 
                expectedInitValType = new IntType();
                break;
            case MultiDimArrayType<PrimitiveType> arrayDecl when arrayDecl.baseType.TypeTag=="bool":
                expectedInitValType = new BoolType();
                break;
            default:
                throw new Exception(
                    String.Format(
                        "{0}:{1} SemanticError: The array type could not be resolved incorrectly"
                    )
                );
        }

        for (int i = 0; i < multiDimArray.initialValues.Length; i++) {
            for (int j = 0; j < multiDimArray.initialValues[i].Length; j++) {
                ExprAST initialValue = multiDimArray.initialValues[i][j];
                initialValue.accept(this);

                SimpleType actualInitValType = initialValue.type;
                if (!sameTypes(expectedInitValType, actualInitValType)) {
                    errorMsgs.Add(
                        String.Format(
                            "{0}:{1} SemanticError: The initial value must be of type {2}",
                            initialValue.lineNumber, initialValue.columnNumber,
                            simpleTypeToString(actualInitValType)
                        )
                    );
                }
            }
        }
    }

    public override void visit(FunctionAST function) { throw new NotImplementedException("This visit is not used."); }
    public override void visit(ParameterAST parameter) { throw new NotImplementedException("This visit is not used."); }
    public override void visit(BlockAST block) { throw new NotImplementedException("This visit is not used."); }
    public override void visit(ConditionalAST conditional) { throw new NotImplementedException("This visit is not used."); }
    public override void visit(WhileLoopAST whileLoop) { throw new NotImplementedException("This visit is not used."); }
    public override void visit(AssignAST assign) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(MultiAssignAST multiAssign) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(MultiAssignCallAST multiAssignCall) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(ArrayAssignAST arrayAssign) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(MultiDimArrayAssignAST multiDimArrayAssign) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(ReturnAST returnStmt) { throw new NotImplementedException("This visit is not used"); }
    public override void visit(ProcedureCallAST procedureCall) { throw new NotImplementedException("This visit is not used"); }
}