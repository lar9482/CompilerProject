function1:
    movI r10, 1000
    movI r11, 2000
    movI r12, 3000
    jmpReg rRET

function2:
    movI r1, 10
    movI r2, 20
    movI r3, 30
    jmpReg rRET

main:
    jmpL function1
    movI rRET, 0

    movI r4, 20
    jmpL_Reg r4
    movI rRET, 0
