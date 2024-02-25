loopBody:

    printw_int r1
    subI r1, 1
    bGt r1, r2, loopBody
    jmp exit

main:
    movI r1, 100
    movI r2, 0

    jmp loopBody

exit: