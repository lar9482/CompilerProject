
/*
 * Basically the wrapper for the symbol tables.
 * Each symbol table represents a "scope" environment.
 * 
 * For example, each function block, if-statement block, and while loop block will
 * have a different symbol table.
 */
using CompilerProj.Types;

internal sealed class Context {

    private Stack<SymbolTable> environment;

    internal Context() {
        this.environment = new Stack<SymbolTable>();
    }

    internal LangType? lookup(string identifier) {
        SymbolTable? currTable = environment.Peek();
        LangType? type = null;
        while (currTable != null) {
            type = currTable.lookup(identifier);
            if (type != null) {
                return type;
            }

            currTable = currTable.parentTable;
        }

        return type;
    }

    internal void put(String identifier, LangType type) {
        environment.Peek().put(identifier, type);
    }

    internal void push() {
        if (environment.Count == 0) 
            environment.Push(new SymbolTable(null));
        else 
            environment.Push(new SymbolTable(environment.Peek()));
    }

    internal void pop() {
        environment.Pop();
    }
}