module RiscV.NET.CPU



type CPU(code: array<uint8>) =
    /// The initial DRAM size is 128MB
    let __DRAM_SIZE: uint64 = 1024UL * 1024UL * 128UL

    /// RISC-V has 32 registers
    let __Regs: array<uint64> = Array.zeroCreate<uint64> (32)

    /// pc register contains the memory address of next instruction
    let mutable __PC: uint64 = 0UL

    /// memory, a byte-array. There is no memory in real CPU.
    let __Dram: array<uint8> = code

    do
        // The stack pointer register sp (aka x2) should point to the top address of DRAM
        __Regs[2] <- __DRAM_SIZE - 1UL

    /// CPU use pc as a base address to fetch 4 continous bytes from DRAM, since RISC-V instruction is 32-bit.
    /// Here, we read the uint8 on [__PC; __PC + 1; __PC + 2; __PC + 3] and build up a uint32.
    member public this.Fetch() : uint32 =
        let index = int (__PC)

        (__Dram[index] |> uint32)
        ||| ((__Dram[index + 1] |> uint32) <<< 8)
        ||| ((__Dram[index + 2] |> uint32) <<< 16)
        ||| ((__Dram[index + 3] |> uint32) <<< 24)

    member public this.Execute(inst: uint32) =
        // decode as R-type
        let opcode = inst &&& 0x7fu
        let rd = (inst >>> 7) &&& 0x1fu |> int
        let rs1 = (inst >>> 15) &&& 0x1fu |> int
        let rs2 = (inst >>> 20) &&& 0x1fu |> int
        let funct3 = (inst >>> 12) &&& 0x7u
        let funct3 = (inst >>> 25) &&& 0x7fu

        // x0 is hardwired zero
        __Regs[0] <- 0UL

        // execute stage
        match opcode with
        // addi
        | 0x13u -> __Regs[rd] <- __Regs[rs1] + (((inst &&& 0xfff0_0000u) |> int64 >>> 20) |> uint64)

        // add
        | 0x33u -> __Regs[rd] <- __Regs[rs1] + __Regs[rs2]

        | _ -> failwith $"Invalid opcode: {opcode}"

    member public this.DumpRegisters() =
        let RVABI =
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

        printfn $"o- Registers"
        __Regs[0] <- 0UL

        for i in 0..4..31 do
            printfn
                $"x{i}({RVABI[i]}) = 0x%0x{__Regs[i]}\tx{i + 1}({RVABI[i + 1]}) = 0x%0x{__Regs[i + 1]}\tx{i + 2}({RVABI[i + 2]}) = 0x%0x{__Regs[i + 2]}\tx{i + 3}({RVABI[i + 3]}) = 0x%0x{__Regs[i + 3]}\n"

    member public this.Run() =
        while __PC < (__Dram.Length |> uint64) do
            this.Fetch() |> this.Execute
            __PC <- __PC + 4UL
