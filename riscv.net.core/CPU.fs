module riscv.net.core.CPU

open System.Diagnostics
open riscv.net.core.Bus
open riscv.net.core.Exception

open Param
open riscv.net.core.Numeric

type CPU (code : array<uint8>) as self =

    [<DefaultValue>]
    val mutable public PC : uint64

    let __regs : array<uint64> = Array.zeroCreate<uint64> (32)
    let __bus : Bus = Bus(code)

    do
        __regs[2] <- DRAM_END
        self.PC <- DRAM_BASE

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

    member public _.REGS = __regs

    member public _.Load (addr : uint64, size : uint64) : uint64 = __bus.Load(addr, size)

    member public _.Store (addr : uint64, size : uint64, value : uint64) : unit = __bus.Store(addr, size, value)

    member public this.Fetch () : uint64 = __bus.Load(this.PC, 32UL)

    member inline private this.UpdatePC () : uint64 = this.PC + 4UL

    member public this.Execute (inst : uint64) : uint64 =
        let opcode = inst &&& 0x0000007FUL
        let rd = int ((inst &&& 0x00000F80UL) >>> 7)
        let rs1 = int ((inst &&& 0x000F8000UL) >>> 15)
        let rs2 = int ((inst &&& 0x01F00000UL) >>> 20)
        let funct3 = (inst &&& 0x00007000UL) >>> 12
        let funct7 = (inst &&& 0xFE000000UL) >>> 25

        match opcode with
        | 0x03UL ->
            let imm = uint64 ((inst |> int32 |> int64) >>> 20)
            let addr = UInt64.WrappingAdd (__regs[rs1], imm)

            match funct3 with
            // LB
            | 0x0UL -> __regs[rd] <- this.Load(addr, 8UL) |> int8 |> int64 |> uint64

            // LH
            | 0x1UL -> __regs[rd] <- this.Load(addr, 16UL) |> int16 |> int64 |> uint64

            // LW
            | 0x2UL -> __regs[rd] <- this.Load(addr, 32UL) |> int32 |> int64 |> uint64

            // LD
            | 0x3UL -> __regs[rd] <- this.Load(addr, 64UL)

            // LBU
            | 0x4UL -> __regs[rd] <- this.Load(addr, 8UL)

            // LHU
            | 0x5UL -> __regs[rd] <- this.Load(addr, 16UL)

            // LWU
            | 0x6UL -> __regs[rd] <- this.Load(addr, 32UL)
            | _ -> raise (IllegalInstruction(inst))

            this.UpdatePC()

        | 0x13UL ->
            let imm = ((inst &&& 0xFFF00000UL) |> int32 |> int64 >>> 20) |> uint64
            let shamt = (imm &&& 0x3FUL) |> uint32

            match funct3 with
            | 0x0UL -> __regs[rd] <- UInt64.WrappingAdd (__regs[rs1], imm)

            // SLLI
            | 0x1UL -> __regs[rd] <- __regs[rs1] <<< (shamt |> int32)

            // SLTI
            | 0x2UL -> __regs[rd] <- if (__regs[rs1] |> int64) < (imm |> int64) then 1UL else 0UL

            // SLTIU
            | 0x3UL -> __regs[rd] <- if __regs[rs1] < imm then 1UL else 0UL

            // XORI
            | 0x4UL -> __regs[rd] <- __regs[rs1] ^^^ imm

            // SRLI & SRAI
            | 0x5UL ->
                match (funct7 >>> 1) with
                | 0x00UL ->
                    __regs[rd] <-
                        UInt32.WrappingShr (__regs[rs1] |> uint32, shamt)
                        |> int32
                        |> int64
                        |> uint64
                | 0x10UL -> __regs[rd] <- Int32.WrappingShr (__regs[rs1] |> int32, shamt |> int) |> int64 |> uint64
                | _ -> raise (IllegalInstruction(inst))

            // ORI
            | 0x6UL -> __regs[rd] <- __regs[rs1] ||| imm

            // ANDI
            | 0x7UL -> __regs[rd] <- __regs[rs1] &&& imm

            | _ -> raise (IllegalInstruction(inst))

            this.UpdatePC()

        // AUIPC
        | 0x17UL ->
            __regs[rd] <- UInt64.WrappingAdd (this.PC, ((inst &&& 0xFFFFF000UL) |> int32 |> int64 |> uint64))
            this.UpdatePC()

        | 0x1BUL ->
            let imm = ((inst |> int32 |> int64) >>> 20) |> uint64
            let shamt = (imm &&& 0x1FUL) |> int

            match funct3 with
            // ADDIW
            | 0x0UL -> __regs[rd] <- UInt64.WrappingAdd (__regs[rs1], imm) |> int32 |> int64 |> uint64

            // SLLIW
            | 0x1UL -> __regs[rd] <- UInt64.WrappingShr (__regs[rs1], shamt |> uint32) |> int32 |> int64 |> uint64

            // SRLIW & SRAIW
            | 0x5UL ->
                match funct7 with
                | 0x00UL ->
                    __regs[rd] <- UInt32.WrappingShr ((__regs[rs1] |> uint32), shamt |> uint32) |> int32 |> int64 |> uint64
                | 0x20UL -> __regs[rd] <- Int32.WrappingShr ((__regs[rs1] |> int32), shamt) |> int64 |> uint64
                | _ -> raise (IllegalInstruction(inst))

            | _ -> raise (IllegalInstruction(inst))

            this.UpdatePC()

        | 0x23UL ->
            let imm =
                (((inst &&& 0xFE000000UL) |> int32 |> int64 >>> 20) |> uint64)
                ||| ((inst >>> 7) &&& 0x1FUL)

            let addr = UInt64.WrappingAdd (__regs[rs1], imm)

            match funct3 with
            // SB
            | 0x0UL -> this.Store(addr, 8UL, __regs[rs2])

            // SH
            | 0x1UL -> this.Store(addr, 16UL, __regs[rs2])

            // SW
            | 0x2UL -> this.Store(addr, 32UL, __regs[rs2])

            // SD
            | 0x3UL ->
                // printfn $"inst = {inst}, imm = {imm}, addr = {addr}"
                this.Store(addr, 64UL, __regs[rs2])

            | _ -> raise (UnreachableException())

            this.UpdatePC()

        | 0x33UL ->
            let shamt = ((__regs[rs2] &&& 0x3FUL) |> uint64) |> uint32

            match (funct3, funct7) with
            | (0x0UL, 0x00UL) -> __regs[rd] <- UInt64.WrappingAdd (__regs[rs1], __regs[rs2])
            | _ -> raise (IllegalInstruction(inst))

            this.UpdatePC()

        // LUI
        | 0x37UL ->
            __regs[rd] <- (inst &&& 0xFFFFF000UL) |> int32 |> int64 |> uint64
            this.UpdatePC()

        | 0x3BUL ->
            let shamt = (__regs[rs2] &&& 0x1FUL) |> uint32

            match (funct3, funct7) with

            // ADDW
            | (0x0UL, 0x00UL) ->
                __regs[rd] <- UInt64.WrappingAdd (__regs[rs1], __regs[rs2]) |> int32 |> int64 |> uint64

            // SUBW
            | (0x0UL, 0x20UL) -> __regs[rd] <- UInt64.WrappingSub ((__regs[rs1], (__regs[rs2]))) |> int32 |> uint64

            // SLLW
            | (0x1UL, 0x00UL) ->
                __regs[rd] <- UInt32.WrappingShl ((__regs[rs1] |> uint32), shamt) |> int32 |> uint64

            // SRLW
            | (0x5UL, 0x00UL) ->
                __regs[rd] <- UInt32.WrappingShr (__regs[rs1] |> uint32, shamt) |> int32 |> uint64

            // SRAW
            | (0x5UL, 0x20UL) -> __regs[rd] <- ((__regs[rs1] |> int32) >>> (shamt |> int32)) |> uint64

            | _ -> raise (IllegalInstruction(inst))

            this.UpdatePC()

        | 0x63UL ->
            let imm =
                (((inst &&& 0x80000000UL) |> int32 |> int64 >>> 19) |> uint64)
                ||| ((inst &&& 0x80UL) <<< 4)
                ||| ((inst >>> 20) &&& 0x7E0UL)
                ||| ((inst >>> 7) &&& 0x1EUL)

            match funct3 with

            // BEQ
            | 0x0UL ->
                if __regs[rs1] = __regs[rs2] then
                    UInt64.WrappingAdd (this.PC, imm)
                else
                    this.UpdatePC()

            // BNE
            | 0x1UL ->
                if __regs[rs1] <> __regs[rs2] then
                    UInt64.WrappingAdd (this.PC, imm)
                else
                    this.UpdatePC()

            // BLT
            | 0x4UL ->
                if ((__regs[rs1] |> int64) < (__regs[rs2] |> int64)) then
                    UInt64.WrappingAdd (this.PC, imm)
                else
                    this.UpdatePC()

            // BGE
            | 0x5UL ->
                if ((__regs[rs1] |> int64) >= (__regs[rs2] |> int64)) then
                    UInt64.WrappingAdd (this.PC, imm)
                else
                    this.UpdatePC()

            // BLTU
            | 0x6UL ->
                if __regs[rs1] < __regs[rs2] then
                    UInt64.WrappingAdd (this.PC, imm)
                else
                    this.UpdatePC()

            // BGEU
            | 0x7UL ->
                if __regs[rs1] >= __regs[rs2] then
                    UInt64.WrappingAdd (this.PC, imm)
                else
                    this.UpdatePC()

            | _ -> raise (IllegalInstruction(inst))

        // JALR
        | 0x67UL ->
            let imm = ((((inst &&& 0xFFF00000UL) |> int32) |> int64) >>> 20) |> uint64
            let new_pc = (UInt64.WrappingAdd(__regs[rs1], imm)) &&& (~~~ 1UL)
            __regs[rd] <- this.PC + 4UL

            printfn $"t = {this.PC + 4UL}, imm = {imm}, new_pc = {new_pc}, __regs[rd] = {__regs[rd]}"

            new_pc

        // JAL
        | 0x6fUL ->
            __regs[rd] <- this.PC + 4UL

            let imm =
                (((inst &&& 0x80000000UL) |> int32 |> int64 >>> 11) |> uint64)
                ||| (inst &&& 0xFF000UL)
                ||| ((inst >>> 9) &&& 0x800UL)
                ||| ((inst >>> 20) &&& 0x7FEUL)

            UInt64.WrappingAdd (this.PC, imm)

        | _ -> raise (IllegalInstruction(inst))
