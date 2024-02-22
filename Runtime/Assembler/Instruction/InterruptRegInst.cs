namespace Compiler.Runtime;

public class InterruptRegInst : Inst {
    public string command { get; }
    public string reg { get; }
    public InterruptRegInst(string command, string reg, string instName) : base(instName){
        this.command = command;
        this.reg = reg;
    }
}