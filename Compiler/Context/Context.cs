
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

namespace CompilerProj.Context;

public sealed class Context {

    private Stack<SymbolTable> environment;

    public Context() {
        this.environment = new Stack<SymbolTable>();
    }

    public T? lookup<T>(string identifier) where T : Symbol {
        SymbolTable? currTable = environment.Peek();
        T? symbol = null;
        while (currTable != null) {
            symbol = currTable.lookup<T>(identifier);
            if (symbol != null) {
                return symbol;
            }

            currTable = currTable.parentTable;
        }

        return symbol;
    }

    public void put(String identifier, Symbol symbol) {
        environment.Peek().put(identifier, symbol);
    }

    public void push() {
        if (environment.Count == 0) 
            environment.Push(new SymbolTable(null));
        else 
            environment.Push(new SymbolTable(environment.Peek()));
    }

    public void push(SymbolTable table) {
        environment.Push(table);
    }

    public SymbolTable pop() {
        return environment.Pop();
    }
}