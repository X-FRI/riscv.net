module riscv.net.core.CPU

module Add =
    let (|+|) (a: uint64) (b: uint64) : uint64 =
        let sum = a + b
        if sum < a || sum < b then a &&& b else sum


open Add
open System

/// CPU (Center Process Unit) is one of the core components of a computer.
/// It consist of a 64-bit pc register, 32 64-bit integer registers and a DRAM as a vector of u8.
type CPU(__code: Collections.Generic.List<uint64>) =
    let __regs: array<uint64> = Array.zeroCreate<uint64> (32)
    let __pc: uint64 = 0UL
    let __dram = __code

    do
        // the stack pointer register sp (aka x2) should point to the top address of DRAM
        __regs[2] <- CPU.DRAM_SIZE - 1UL

    static member public RVABI =
        [| "zero"
           "ra"
           "sp"
           "gp"
           "tp"
           "t0"
           "t1"
           "t2"
           "s0"
           "s1"
           "a0"
           "a1"
           "a2"
           "a3"
           "a4"
           "a5"
           "a6"
           "a7"
           "s2"
           "s3"
           "s4"
           "s5"
           "s6"
           "s7"
           "s8"
           "s9"
           "s10"
           "s11"
           "t3"
           "t4"
           "t5"
           "t6" |]

    /// The size of memory to initialize our CPU (128MB).
    static member public DRAM_SIZE: uint64 = 1024UL * 1024UL * 128UL

    /// RISC-V has 32 registers
    member public _.REGS = __regs

    /// PC register contains the memory address of next instruction
    /// Initialize it to 0, means will start fetch instruction from address 0.
    member public _.PC: uint64 = 0UL

    /// Memory, a byte-array. There is no memory in real CPU.
    member public _.DRAM: Collections.Generic.List<uint64> = __dram

    /// CPU use pc as a base address to fetch 4 continous bytes from DRAM,
    /// since RISC-V instruction is 32-bit. read the u8 on [pc, pc+1, pc+2, pc+3] and build up a u32.
    /// This emulator support little-endianness.
    member private this.Fetch() : uint32 =
        let index: int = int (__pc) in

        uint32 (__dram[index])
        ||| uint32 (__dram[index + 1] <<< 8)
        ||| uint32 (__dram[index + 2] <<< 16)
        ||| uint32 (__dram[index + 3] <<< 24)

    /// riscv currently has four basic instruction encoding formats
    member private this.Execute(instruction: uint32) =
        // decode as r-type
        let opcode = uint (instruction &&& 0x7Fu) in
        let rd = int ((instruction >>> 7) &&& 0x1Fu) in
        let rs1 = int ((instruction >>> 15) &&& 0x1Fu) in
        let rs2 = int ((instruction >>> 20) &&& 0x1Fu) in
        let funct3 = int ((instruction >>> 12) &&& 0x7u) in
        let funct7 = int ((instruction >>> 25) &&& 0x7Fu) in

        // x0 is hardwired zero
        __regs[0] <- 0UL

        // Executa stage.
        // These two instructions ignore arithmetic overflow errors (arithmetic overflow),
        // overflow bit (bit) will be discarded directly
        match opcode with
        // addi
        | 0x13u -> __regs[rd] <- __regs[rs1] |+| uint64 ((int64 (instruction &&& 0xFFF00000u)) >>> 20l)
        // add
        | 0x33u -> __regs[rd] <- __regs[rs1] |+| __regs[rs2]
        | _ -> failwith ($"Invalid opcode: %02X{opcode}")

    /// View the state of the registers to verify that the CPU executed instructions correctly.
    member private this.DumpRegisters() =
        printfn ($"o- REGISTERS")

        __regs[0] <- 0UL

        let mutable i = 0

        while i <> 32 do
            printfn
                $"""| x{i}({CPU.RVABI[i]}) x{i + 1}({CPU.RVABI[i]}) = {__regs[i]} | x{i + 2}({CPU.RVABI[i + 2]}) = {__regs[i + 2]} | x{i + 3}({CPU.RVABI[i + 3]}) = {__regs[i + 3]} |"""

            i <- i + 4
