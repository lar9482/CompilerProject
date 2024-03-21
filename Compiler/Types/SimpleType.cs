using CompilerProj.Types;

internal abstract class SimpleType : LangType {
}

internal abstract class PrimitiveType : SimpleType {
}

internal sealed class IntType : PrimitiveType {
    internal override string TypeTag => "int";
}

internal sealed class BoolType : PrimitiveType {
    internal override string TypeTag => "bool";
}

internal sealed class ArrayType<T> : SimpleType where T : PrimitiveType {
    internal override string TypeTag => "array";
    internal T baseType { get; }

    internal ArrayType(T baseType) {
        this.baseType = baseType;
    }
}

internal sealed class MultiDimArrayType<T> : SimpleType where T : PrimitiveType {
    internal override string TypeTag => "multiDimArray";
    internal T baseType { get; }

    internal MultiDimArrayType(T baseType) {
        this.baseType = baseType;
    }
}