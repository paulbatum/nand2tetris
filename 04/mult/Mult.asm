// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Mult.asm

// Multiplies R0 and R1 and stores the result in R2.
// (R0, R1, R2 refer to RAM[0], RAM[1], and RAM[2], respectively.)
//
// This program only needs to handle arguments that satisfy
// R0 >= 0, R1 >= 0, and R0*R1 < 32768.

    // initialize R2 to zero
    D=0
    @R2
    M=D
    // initialze n to R0
    @R0
    D=M
    @n
    M=D
    // TODO: shortcircuit if both R0 and R1 are zero
(LOOP)
    // if n == 0 jump to END
    @n
    D=M
    @END
    D;JEQ
    // R2 = R2 + R1
    @R2
    D=M
    @R1
    D=D+M
    @R2
    M=D
    // n = n - 1
    @n
    M=M-1
    // goto LOOP
    @LOOP
    0;JMP
(END)
    @END
    0;JMP