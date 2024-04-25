namespace CompilerProj.Types;

public abstract class StmtType : LangType {
}

/*
 * Unit types refer to statements that don't return anything, and the next statement can be executed
 */
public sealed class UnitType : StmtType {
    public override string TypeTag => "unit";
}

/*
 * Terminates types refer to statements that don't return anything, and the next statement can not be executed.
 */
public sealed class TerminateType : StmtType {
    public override string TypeTag => "terminate";
}

/*
 * Unintialized stmt types refer to statements that have not had their types inferred yet.
 */
public sealed class UninitializedStmtType: StmtType {
    public override string TypeTag => "uninitialized";
}