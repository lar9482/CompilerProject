namespace CompilerProj.Types;

internal abstract class LangType {
    internal abstract string TypeTag { get; }
}

internal abstract class PrimitiveType : LangType {
}

internal sealed class IntType : PrimitiveType {
    internal override string TypeTag => "int";
}

internal sealed class BoolType : PrimitiveType {
    internal override string TypeTag => "bool";
}

internal sealed class ArrayType<T> : LangType where T : PrimitiveType {
    internal override string TypeTag => "array";
    internal T baseType { get; }

    internal ArrayType(T baseType) {
        this.baseType = baseType;
    }
}

internal sealed class MultiDimArrayType<T> : LangType where T : PrimitiveType {
    internal override string TypeTag => "multiDimArray";
    internal T baseType { get; }

    internal MultiDimArrayType(T baseType) {
        this.baseType = baseType;
    }
}