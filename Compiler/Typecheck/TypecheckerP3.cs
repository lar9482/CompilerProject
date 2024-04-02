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

    public override void visit(ProgramAST program) { 
        if (program.scope == null) {
            errorMsgs.Add("Semantic Error: Top level scope was not initialized");
            return;
        }

        context.push(program.scope);

        foreach(FunctionAST function in program.functions) {
            function.accept(this);
        }

        program.scope = context.pop();
    }

    public override void visit(VarDeclAST varDecl) { 
        varDecl.type = new UnitType();
        initializeVarDecl(varDecl);
        checkVarDecl(varDecl);
    }

    public override void visit(MultiVarDeclAST multiVarDecl) { 
        multiVarDecl.type = new UnitType();
        initializeMultiVarDecl(multiVarDecl);
        checkMultiVarDecl(multiVarDecl);
    }

    public override void visit(MultiVarDeclCallAST multiVarDeclCall) { 
        multiVarDeclCall.type = new UnitType();
        initializeMultiVarDeclCall(multiVarDeclCall);
        checkMultiVarDeclCall(multiVarDeclCall);
    }

    public override void visit(ArrayDeclAST array) { 
        array.type = new UnitType();
        initializeArrayDecl(array);
        checkArrayDecl(array);
    }

    public override void visit(MultiDimArrayDeclAST multiDimArray) { 
        multiDimArray.type = new UnitType();
        initializeMultiDimArrayDecl(multiDimArray);
        checkMultiDimArrayDecl(multiDimArray);
    }

    public override void visit(FunctionAST function) { 
        context.push();
        foreach (ParameterAST param in function.parameters) {
            param.accept(this);
        }

        SymbolReturn symbolReturn = new SymbolReturn(
            function.returnTypes.ToArray()
        );
        context.put("return", symbolReturn);

        function.block.accept(this);
        function.scope = context.pop();
    }
}