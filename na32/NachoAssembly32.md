# Nacho Assembly 32


## Registers
### A Register
8 bit register
### B Register
8 bit register
### Stack
Tied to file pointer, 64 bit

## Memory
65,535 byte memory

## Instructions
### 00 - Noop
    Perform no operations
### 01 - ADD
    Read one int32 and sum it to the A register
### 02 - SUB
    Read one int32 and subtract it from the A register
### 03 - MUL
    Read pme int32 and multiply it with the register
### 04 - DIV
    Read one int32 and divide it from the A register
### 05 - JEZ
	If A register == 0
	Read 4 bytes as interger and jump to that address in the program
### 06 - JNZ
	If A register != 0
	Read 4 bytes as interger and jump to that address in the program
### 07 - JEQ
    If A register == B register
	Read 4 bytes as interger and jump to that address in the program
### 08 - OUT
    Outputs register A as a character in ascii
### 09 - LDA
    Read one int32 and load it to the A register
### 0A - LDB
    Read one int32 and load it to the B register
### 0B - MVA
    Read one int32, move the A register to a memory location using that int32 as a pointer
### 0C - MVB
    Read one int32, move the B register to a memory location using that int32 as a pointer
### 0D - OUI
	Outputs register A as an interger
### 0E - CLL
### 0F - RET
### 10 - RDA
    Read one int32, Read a memory location using that int32 as a pointer to register A
### 11 - RDB
    Read one int32, Read a memory location using that int32 as a pointer to register B
### 12 - JMP
    Read one int32 and jump to that location
### 99 - EOP
    End of Program