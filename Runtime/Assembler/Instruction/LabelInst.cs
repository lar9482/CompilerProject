namespace Compiler.Runtime;

public class LabelInst : Inst {
    public string label { get; }
    
    public LabelInst(string label, string instName) : base(instName){
        this.label = label;
    }
}