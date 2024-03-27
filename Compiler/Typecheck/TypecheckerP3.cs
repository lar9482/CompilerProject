using CompilerProj.Context;
using CompilerProj.Visitors;
using CompilerProj.Types;
using System.Diagnostics;

/*
 * This pass will typecheck the function bodies
 */
public sealed class TypecheckerP3 : TypeChecker {
    public TypecheckerP3() {
    }

    // Top level nodes
    public override void visit(ProgramAST program) { 
        if (program.scope == null) {
            errorMsgs.Add("Semantic Error: Top level scope was not initialized");
            return;
        }

        context.push(program.scope);

        foreach(DeclAST decl in program.declarations) {
            decl.accept(this);
        }

        foreach(FunctionAST function in program.functions) {
            function.accept(this);
        }
        program.scope = context.pop();
    }

    public override void visit(VarDeclAST varDecl) { 
        SymbolVariable symbol = (SymbolVariable) lookUpSymbolFromContext(
            varDecl.name, varDecl.lineNumber, varDecl.columnNumber
        );
        varDecl.type = new UnitType();

        if (varDecl.initialValue == null) {
            return;
        }
        varDecl.initialValue.accept(this);

        if (!sameTypes(symbol.type, varDecl.initialValue.type)) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: The initial value doesn't match with the declaration type",
                    varDecl.lineNumber, 
                    varDecl.columnNumber
                )
            );
        }
    }

    public override void visit(MultiVarDeclAST multiVarDecl) { }
    public override void visit(MultiVarDeclCallAST multiVarDeclCall) { }
    public override void visit(ArrayDeclAST array) { }
    public override void visit(MultiDimArrayDeclAST multiDimArray) { }
    public override void visit(FunctionAST function) { }
    public override void visit(ParameterAST parameter) { }
    public override void visit(BlockAST block) { }

    // Stmt nodes
    public override void visit(AssignAST assign) { }
    public override void visit(MultiAssignAST multiAssign) { }
    public override void visit(MultiAssignCallAST multiAssignCall) { }
    public override void visit(ArrayAssignAST arrayAssign) { }
    public override void visit(MultiDimArrayAssignAST multiDimArrayAssign) { }
    public override void visit(ConditionalAST conditional) { }
    public override void visit(WhileLoopAST whileLoop) { }
    public override void visit(ReturnAST returnStmt) { }
    public override void visit(ProcedureCallAST procedureCall) { }
}