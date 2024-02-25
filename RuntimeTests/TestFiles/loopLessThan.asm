loopBody:
    add r3, r1
    addI r1, 1
    bLt r1, r2, loopBody
    jmp exit

main:
    movI r1, 0
    movI r2, 101

    jmp loopBody

exit: