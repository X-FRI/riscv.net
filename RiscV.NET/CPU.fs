module RiscV.Net.CPU

open Dram
open Bus

type CPU(__code: array<uint8>) =
    let __regs: array<uint64> = Array.zeroCreate<uint64> (32)
    let __bus: Bus = Bus(__code)
    let mutable __pc: uint64 = Dram.BASE

    do __regs[2] <- Dram.END

    member public this.Fetch() : uint64 = __bus.Load(__pc, 32UL)

    member private this.UpdatePC() = __pc <- __pc + 4UL

    member public this.Execute(inst: uint64) : unit =
        let opcode = inst &&& 0x0000007fUL
        let rd = ((inst &&& 0x00000f80UL) >>> 7) |> int
        let rs1 = ((inst &&& 0x000f8000UL) >>> 15) |> int
        let rs2 = ((inst &&& 0x01f00000UL) >>> 20) |> int
        let funct3 = (inst &&& 0x00007000UL) >>> 12
        let funct7 = (inst &&& 0xfe000000UL) >>> 25

        __regs[0] <- 0UL

        match opcode with
        | 0x13UL ->
            let imm = ((inst &&& 0xfff00000UL) |> int32 |> int64 >>> 20) |> uint64
            let shamt = (imm &&& 0x3fUL) |> uint32

            match funct3 with
            // addi
            | 0x0UL -> __regs[rd] <- (__regs[rs1] + imm)
            | _ -> failwith $"Illegal instruction: %X{inst}"

        | 0x33UL ->
            // "SLL, SRL, and SRA perform logical left, logical right, and arithmetic right
            // shifts on the value in register rs1 by the shift amount held in register rs2.
            // In RV64I, only the low 6 bits of rs2 are considered for the shift amount."
            let shamt = ((__regs[rs2] &&& 0x3fUL) |> uint64) |> uint32

            match (funct3, funct7) with
            // add
            | (0x0UL, 0x00UL) -> __regs[rd] <- (__regs[rs1] + __regs[rs2])
            | _ -> failwith $"Illegal instruction: %X{inst}"

        | _ -> failwith $"Illegal instruction: %X{inst}"

        this.UpdatePC()


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
        __regs[0] <- 0UL

        for i in 0..4..31 do
            printfn
                $"x{i}({RVABI[i]}) = 0x%0x{__regs[i]}\tx{i + 1}({RVABI[i + 1]}) = 0x%0x{__regs[i + 1]}\tx{i + 2}({RVABI[i + 2]}) = 0x%0x{__regs[i + 2]}\tx{i + 3}({RVABI[i + 3]}) = 0x%0x{__regs[i + 3]}\n"

    member public this.Run() =
        try
            while true do
                this.Fetch() |> this.Execute
        with _ ->
            this.DumpRegisters()
