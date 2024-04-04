namespace CompilerProj.Context;

/*
 * Implementation of a symbol on a scope level.
 * This shouldn't be used directly in the visitors.
 * It should be used with Context.cs
 */
public sealed class SymbolTable {

    private Dictionary<string, Symbol> table;
    public SymbolTable? parentTable;
    
    public SymbolTable(SymbolTable? parentTable) {
        this.table = new Dictionary<string, Symbol>();
        this.parentTable = parentTable;
    }

    public void put(string identifier, Symbol symbol) {
        table.Add(identifier, symbol);
    }

    public T? lookup<T>(string identifier) where T : Symbol{
        Symbol? symbol;
        if (table.TryGetValue(identifier, out symbol)) {
            if (symbol is T specifiedSymbol) {
                return specifiedSymbol;
            }
        }

        return null;
    }

    public void removeIdentifier(string identifier) {
        table.Remove(identifier);
    }
}