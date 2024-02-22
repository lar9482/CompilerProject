lb r10, 0[r1]
lw r11, 0[r2]
sb r10, 0[r1]
sw r11, 0[r2]

jmp unconditionalJump
unconditionalJump:

bLez rZERO, zeroJump
zeroJump: