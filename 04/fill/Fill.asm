// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Fill.asm

// Runs an infinite loop that listens to the keyboard input.
// When a key is pressed (any key), the program blackens the screen,
// i.e. writes "black" in every pixel;
// the screen should remain fully black as long as the key is pressed.
// When no key is pressed, the program clears the screen, i.e. writes
// "white" in every pixel;
// the screen should remain fully clear as long as no key is pressed.

// Put your code here.
// (LOOP)
//     @KBD
//     D=M
//     @CLEAR
//     D;JEQ
//     @SCREEN
//     M=-1
//     @LOOP
//     0;JMP
// (CLEAR)
//     @SCREEN
//     M=0
//     @LOOP
//     0;JMP

    @offset
    M=0
(LOOP)
    @KBD
    D=M
    @CLEAR
    D;JEQ
    @offset
    D=M
    @SCREEN
    A=A+D
    M=-1
    @NEXT
    0;JMP
(CLEAR)
    @offset
    D=M
    @SCREEN
    A=A+D
    M=0
(NEXT)
    @offset
    M=M+1
    D=M
    @8192
    D=D-A
    @LOOP
    D;JLT
    @offset
    M=0
    @LOOP
    0;JMP




