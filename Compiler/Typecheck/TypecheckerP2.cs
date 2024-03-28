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

        checkVarDecl(varDecl);
    }

    public override void visit(MultiVarDeclAST multiVarDecl) { 
        multiVarDecl.type = new UnitType();

        checkMultiVarDecl(multiVarDecl);
    }

    public override void visit(MultiVarDeclCallAST multiVarDeclCallAST) {
        multiVarDeclCallAST.type = new UnitType();

        checkMultiVarDeclCall(multiVarDeclCallAST);
    }

    public override void visit(ArrayDeclAST array) { 
        array.type = new UnitType();

        checkArrayDecl(array);
    }

    public override void visit(MultiDimArrayDeclAST multiDimArray) { 
        multiDimArray.type = new UnitType();

        checkMultiDimArrayDecl(multiDimArray);
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