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

    public Symbol? lookup(string identifier) {
        Symbol? symbol;
        table.TryGetValue(identifier, out symbol);
        
        return symbol;
    }

    public void removeIdentifier(string identifier) {
        table.Remove(identifier);
    }
}