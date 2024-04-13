namespace CompilerProj.IRInterpreter.ExprStack;

/**
 * While traversing the IR tree, we require a stack in order to hold a number of single-word
 * values (e.g., to evaluate binary expressions). This also keeps track of whether a value was
 * created by a TEMP or MEM, or NAME reference, which is useful when executing moves.
 */
public sealed class ExprStack {
    private Stack<StackItem> stack;

    public ExprStack() {
        this.stack = new Stack<StackItem>();
    }

    public int popValue() {
        int value = stack.Pop().value;
        return value;
    }

    public StackItem pop() {
        return stack.Pop();
    }

    public void pushAddress(int value, int address) {
        stack.Push(
            new StackItem(value, address)
        );
    }

    public void pushTemp(int value, string temp) {
        stack.Push(
            new StackItem(
                StackItemType.TEMP,
                value,
                temp
            )
        );
    }

    public void pushName(int value, string name) {
        stack.Push(
            new StackItem(
                StackItemType.NAME,
                value,
                name
            )
        );
    }
    
    public void pushValue(int value) {
        stack.Push(
            new StackItem(value)
        );
    }
}

public struct StackItem {
    public StackItemType type;
    public int value;
    public int address;

    public string? temp;
    public string? name;

    public StackItem(int value) {
        this.type = StackItemType.COMPUTED;
        this.value = value;
    }

    public StackItem(int value, int address) {
        this.value = value;
        this.address = address;
    }

    public StackItem(StackItemType type, int value, string name) {
        this.type = type;
        this.value = value;
        if (type == StackItemType.TEMP) {
            this.temp = name;
        } else {
            this.name = name;
        }
    }
}

public enum StackItemType {
    COMPUTED,
    MEM,
    TEMP,
    NAME
}