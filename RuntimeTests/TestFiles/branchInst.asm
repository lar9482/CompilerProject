notEqual:
    movI r3, 432
    movI r4, 674674
    jmp exit

isEqual:
    movI r3, 300
    movI r4, 400
    jmp exit

main:
    movI r1, 100
    movI r2, 100
    bEq r1, r2, isEqual
    bNe r1, r2, notEqual

exit:
