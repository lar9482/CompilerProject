using CompilerProj.Types;

/*
 * Implementation of a symbol on a scope level.
 * This shouldn't be used directly in the visitors.
 * It should be used with Context.cs
 */
public sealed class SymbolTable {

    private Dictionary<string, LangType> table;
    public SymbolTable? parentTable;
    
    public SymbolTable(SymbolTable? parentTable) {
        this.table = new Dictionary<string, LangType>();
        this.parentTable = parentTable;
    }

    public void put(string identifier, LangType type) {
        table.Add(identifier, type);
    }

    public LangType? lookup(string identifier) {
        LangType? type;
        table.TryGetValue(identifier, out type);
        
        return type;
    }

    public void removeIdentifier(string identifier) {
        table.Remove(identifier);
    }
}