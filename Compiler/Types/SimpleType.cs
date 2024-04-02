using CompilerProj.Types;

/*
 * A type for denoting expressions.
 */
public abstract class SimpleType : LangType {
}

/*
 * Strict subset of "SimpleType". Used to denote int and bool.
 */
public abstract class PrimitiveType : SimpleType {
}

/*
 * Used to denote expressions that have yet to have their type inferred.
 */
public sealed class UninitializedSimpleType : SimpleType {
    public override string TypeTag => "uninitalized";
}

public sealed class IntType : PrimitiveType {
    public override string TypeTag => "int";
}

public sealed class BoolType : PrimitiveType {
    public override string TypeTag => "bool";
}

/*
 * Used to denote T[] types
 */
public sealed class ArrayType<T> : SimpleType where T : PrimitiveType {
    public override string TypeTag => "array";
    public T baseType { get; }

    public ArrayType(T baseType) {
        this.baseType = baseType;
    }
}

/*
 * Used to denote T[][] types
 */
public sealed class MultiDimArrayType<T> : SimpleType where T : PrimitiveType {
    public override string TypeTag => "multiDimArray";
    public T baseType { get; }

    public MultiDimArrayType(T baseType) {
        this.baseType = baseType;
    }
}