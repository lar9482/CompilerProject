using CompilerProj.Types;

public abstract class SimpleType : LangType {
}

public abstract class PrimitiveType : SimpleType {
}

public sealed class UninitializedSimpleType : SimpleType {
    public override string TypeTag => "uninitalized";
}

public sealed class IntType : PrimitiveType {
    public override string TypeTag => "int";
}

public sealed class BoolType : PrimitiveType {
    public override string TypeTag => "bool";
}

public sealed class ArrayType<T> : SimpleType where T : PrimitiveType {
    public override string TypeTag => "array";
    public T baseType { get; }

    public ArrayType(T baseType) {
        this.baseType = baseType;
    }
}

public sealed class MultiDimArrayType<T> : SimpleType where T : PrimitiveType {
    public override string TypeTag => "multiDimArray";
    public T baseType { get; }

    public MultiDimArrayType(T baseType) {
        this.baseType = baseType;
    }
}