loopBody:
    add r3, r1
    subI r1, 1
    bGt r1, r2, loopBody
    jmp exit

main:
    movI r1, 100
    movI r2, -1

    jmp loopBody

exit: