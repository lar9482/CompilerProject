namespace CompilerProj.Types;

internal abstract class StmtType : LangType {
}

/*
 * Unit types refer to statements that don't return anything, and the next statement can be executed
 */
internal sealed class UnitType : StmtType {
    internal override string TypeTag => "unit";
}

/*
 * Terminates types refer to statements that don't return anything, and the next statement can not be executed.
 */
internal sealed class TerminateType : StmtType {
    internal override string TypeTag => "void";
}