using CompilerProj.Types;

/*
 * Implementation of a symbol on a scope level.
 * This shouldn't be used directly in the visitors.
 * It should be used with Context.cs
 */
internal sealed class SymbolTable {

    internal Dictionary<string, LangType> table;
    internal SymbolTable? parentTable;
    
    internal SymbolTable(SymbolTable? parentTable) {
        this.table = new Dictionary<string, LangType>();
        this.parentTable = parentTable;
    }

    internal void put(string identifier, LangType type) {
        table.Add(identifier, type);
    }

    internal LangType? lookup(string identifier) {
        LangType? type;
        table.TryGetValue(identifier, out type);
        
        return type;
    }

    internal void removeIdentifier(string identifier) {
        table.Remove(identifier);
    }
}