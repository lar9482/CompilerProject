using CompilerProj.Context;
using CompilerProj.Visitors;
using CompilerProj.Types;
using System.Diagnostics;

/*
 * This pass will typecheck the function bodies.
 * 
 * This includes every statement/declaration in the function body,
 * along with expressions associated with these statements.
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
        initializeVarDecl(varDecl);
        checkVarDecl(varDecl);
    }

    public override void visit(MultiVarDeclAST multiVarDecl) { 
        initializeMultiVarDecl(multiVarDecl);
        checkMultiVarDecl(multiVarDecl);
    }

    public override void visit(MultiVarDeclCallAST multiVarDeclCall) { 
        initializeMultiVarDeclCall(multiVarDeclCall);
        checkMultiVarDeclCall(multiVarDeclCall);
    }

    public override void visit(ArrayDeclAST array) { 
        initializeArrayDecl(array);
        checkArrayDecl(array);
    }

    public override void visit(MultiDimArrayDeclAST multiDimArray) { 
        initializeMultiDimArrayDecl(multiDimArray);
        checkMultiDimArrayDecl(multiDimArray);
    }   

    //TODO: Implement these
    public override void visit(ArrayDeclCallAST arrayCall) {
        initializeArrayDeclCall(arrayCall);
        checkArrayDeclCall(arrayCall);
    }

    public override void visit(MultiDimArrayDeclCallAST multiDimArrayCall) {
        initializeMultiDimArrayDeclCall(multiDimArrayCall);
        checkMultiDimArrayDeclCall(multiDimArrayCall);
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

        StmtType blockType = function.block.type;
        if (blockType.TypeTag == "uninitialized" && symbolReturn.returnTypes.Length > 0) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: Not all code paths in {2} will return something.",
                    function.lineNumber, function.columnNumber, function.name
                )
            );
        }

        if (blockType.TypeTag == "unit" && symbolReturn.returnTypes.Length > 0) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: Not all code paths in {2} will return something.",
                    function.lineNumber, function.columnNumber, function.name
                )
            );
        }

        if (blockType.TypeTag == "terminate" && symbolReturn.returnTypes.Length == 0) {
            errorMsgs.Add(
                String.Format(
                    "{0}:{1} SemanticError: There exists a code path in {2} that returns something, when that's not allowed.",
                    function.lineNumber, function.columnNumber, function.name
                )
            );
        }

        
        function.scope = context.pop();
    }
}