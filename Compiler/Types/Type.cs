namespace CompilerProj.Types;

public abstract class LangType {
    public abstract string TypeTag { get; }
}

public abstract class PrimitiveType : LangType {
}

public class IntType : PrimitiveType {
    public override string TypeTag => "int";
}

public class BoolType : PrimitiveType {
    public override string TypeTag => "bool";
}

public class ArrayType<T> : LangType where T : PrimitiveType {
    public override string TypeTag => "array";
    public T baseType { get; }

    public ArrayType(T baseType) {
        this.baseType = baseType;
    }
}

public class MultiDimArrayType<T> : LangType where T : PrimitiveType {
    public override string TypeTag => "multiDimArray";
    public T baseType { get; }

    public MultiDimArrayType(T baseType) {
        this.baseType = baseType;
    }
}