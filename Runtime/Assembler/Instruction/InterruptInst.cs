namespace Compiler.Runtime;

public class InterruptInst : Inst {
    public string command { get; }
    public InterruptInst(string command, string instName) : base(instName){
        this.command = command;
    }
}