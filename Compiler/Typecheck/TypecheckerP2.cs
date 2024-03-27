using CompilerProj.Context;
using CompilerProj.Visitors;

/*
 * This pass will typecheck top level declarations.
 */
public sealed class TypecheckerP2 : TypeChecker {

    public override void visit(ProgramAST program) {
        if (program.scope == null) {
            errorMsgs.Add("Semantic Error: Top level scope was not initialized");
            return;
        } 
        context.push(program.scope);

        foreach(DeclAST decl in program.declarations) {
            decl.accept(this);
        }

        program.scope = context.pop();
    }

    public override void visit(VarDeclAST varDecl) { 
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

    public override void visit(MultiVarDeclAST multiVarDecl) { }
    public override void visit(MultiVarDeclCallAST multiVarDeclCallAST) { }
    public override void visit(ArrayDeclAST array) { }
    public override void visit(MultiDimArrayDeclAST multiDimArray) { }

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