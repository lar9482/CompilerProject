main:
mov r1, r2
add r2, r2
sub r3, r3
mult r4, r4
div r5, r5
or r6, r6
xor r7, r7
not r8, r8
nor r9, r9
sllv r10, r10
srav r11, r11
movI r9, 1000 
movI r9, -1000
addI r1, 100
subI r2, 100
multI r3, 10
divI r4, 10
orI r5, 10
xorI r6, 10
sll r7, 10
sra r8, 10

lb r10, -100[r1]
lw r11, 0[r2]
sb r10, 0[r1]
sw r11, 0[r2]

jmp unconditionalJump
addI r1, 100

unconditionalJump:

jmpReg r1
jmpL_Reg r1

bEq rZERO, r1, zeroJump
zeroJump: