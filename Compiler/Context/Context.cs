
/*
 * Basically the wrapper for the symbol tables, 
 * which implements static scoping via a stack of symbol tables.
 *
 * Each symbol table represents a "scope" environment.
 * 
 * For example, each function block, if-statement block, and while loop block will
 * have a different symbol table.
 */
using CompilerProj.Types;

public sealed class Context {

    private Stack<SymbolTable> environment;

    public Context() {
        this.environment = new Stack<SymbolTable>();
    }

    public LangType? lookup(string identifier) {
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

    public void put(String identifier, LangType type) {
        environment.Peek().put(identifier, type);
    }

    public void push() {
        if (environment.Count == 0) 
            environment.Push(new SymbolTable(null));
        else 
            environment.Push(new SymbolTable(environment.Peek()));
    }

    public void pop() {
        environment.Pop();
    }
}