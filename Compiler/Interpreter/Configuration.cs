

public struct IRConfiguration {
    /** Prefix for argument registers */
    public const string ABSTRACT_ARG_PREFIX = "_ARG";

    /** Prefix for return registers */
    public const string ABSTRACT_RET_PREFIX = "_RV";

    /** Prefix for stack register */
    public const string ABSTRACT_STACK_PREFIX = "_ST";
    
    /** Flag for an out of bounds error */
    public const string OUT_OF_BOUNDS_FLAG = "outOfBounds";

    /** Word size; assumes a 32-bit architecture */
    public const int wordSize = 4;
}