namespace Compiler.Runtime;

public enum TokenType {
    //General purpose registers
    rZERO_Reg, //Register that will hold zero at all times
    r1_Reg,
    r2_Reg,
    r3_Reg,
    r4_Reg,
    r5_Reg,
    r6_Reg,
    r7_Reg,
    r8_Reg,
    r9_Reg,
    r10_Reg,
    r11_Reg,
    r12_Reg,
    r13_Reg,
    r14_Reg,
    r15_Reg,
    r16_Reg,
    rSP_Reg, //Stack pointer register
    rFP_Reg, //Frame pointer register 
    rRET_Reg, //Return pointer register
    rHI_Reg, //Holds truncated result from division
    rLO_Reg, //Holds remainder from truncated result

    //Register instructions(opcode reg, reg)
    mov_Inst, 
    add_Inst,
    sub_Inst,
    mult_Inst,
    div_Inst,
    and_Inst,
    or_Inst,
    xor_Inst,
    not_Inst,
    nor_Inst,
    sllv_Inst,
    srav_Inst,

    //Immediate instructions(opcode reg, integer)
    movI_Inst,
    addI_Inst,
    subI_Inst,
    multI_Inst,
    divI_Inst,
    andI_Inst,
    orI_Inst,
    xorI_Inst,
    sll_Inst,
    sra_Inst,
    
    //Jump instructions with registers and labels(opcode reg, reg, label)
    bEq_Inst, 
    bNe_Inst,

    //Jump instructions (opcode label:) or (opcode reg)
    jmp_Inst, //(opcode label)
    jmpL_Inst, // (opcode label)
    jmpL_Reg_Inst, //(opcode reg)
    jmpRet_Inst, // (opcode reg)

    //Memory instructions (opcode reg, reg(integer))
    lb_Inst,
    lw_Inst,
    sb_Inst,
    sw_Inst,

    //Interrupt instructions
    halt_Inst,
    printw_Int_Inst,
    printw_Hex_Inst,
    printw_Bin_Inst,
    //Other stuff
    identifier,
    integer,

    //Symbols that will appear in the language
    comma,
    colon,
    startBracket,
    endBracket,
    EOF
}