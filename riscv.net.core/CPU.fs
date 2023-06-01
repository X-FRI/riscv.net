module riscv.net.core.CPU

open riscv.net.core.Bus
open riscv.net.core.Exception

module Add =
    let (|+|) (a: uint64) (b: uint64) : uint64 =
        let sum = a + b
        if sum < a || sum < b then a &&& b else sum

open Add
open Param

type CPU(code: array<uint8>) as self =

    [<DefaultValue>]
    val mutable public PC: uint64

    let __regs: array<uint64> = Array.zeroCreate<uint64> (32)
    let __bus: Bus = Bus(code)

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

    member public _.Load(addr: uint64, size: uint64) : uint64 = __bus.Load(addr, size)

    member public _.Store(addr: uint64, size: uint64, value: uint64) : unit = __bus.Store(addr, size, value)

    member public this.Fetch() : uint64 = __bus.Load(this.PC, 32UL)

    member inline private this.UpdatePC() : uint64 = this.PC + 4UL

    member public this.Execute(inst: uint64) : uint64 =
        let opcode = inst &&& 0x0000007FUL
        let rd = int ((inst &&& 0x00000F80UL) >>> 7)
        let rs1 = int ((inst &&& 0x000F8000UL) >>> 15)
        let rs2 = int ((inst &&& 0x01F00000UL) >>> 20)
        let funct3 = (inst &&& 0x00007000UL) >>> 12
        let funct7 = (inst &&& 0xFE000000UL) >>> 25
        
        printfn $"pc = {this.PC}, inst = {inst}, funct3 = {funct3}, funct7 = {funct7}, opcode = {opcode}"

        match opcode with
        | 0x03UL ->
            let imm = uint64 ((inst |> int32 |> int64) >>> 20)
            let addr = __regs[rs1] |+| imm

            match funct3 with
            | 0x0UL ->
                __regs[rd] <- (this.Load(addr, 8UL) |> int8 |> int64 |> uint64)
                this.UpdatePC()
            | 0x1UL ->
                __regs[rd] <- (this.Load(addr, 16UL) |> int16 |> int64 |> uint64)
                this.UpdatePC()
            | 0x2UL ->
                __regs[rd] <- (this.Load(addr, 32UL) |> int32 |> int64 |> uint64)
                this.UpdatePC()
            | 0x3UL ->
                __regs[rd] <- this.Load(addr, 64UL)
                this.UpdatePC()
            | 0x4UL ->
                __regs[rd] <- (this.Load(addr, 8UL))
                this.UpdatePC()
            | 0x5UL ->
                __regs[rd] <- (this.Load(addr, 16UL))
                this.UpdatePC()
            | 0x6UL ->
                __regs[rd] <- (this.Load(addr, 32UL))
                this.UpdatePC()
            | _ -> raise (IllegalInstruction(inst))

        | 0x13UL ->
            let imm = ((inst &&& 0xFFF00000UL) |> int32 |> int64 >>> 20) |> uint64
            let shamt = (imm &&& 0x3FUL) |> uint32

            match funct3 with
            | 0x0UL ->
                __regs[rd] <- __regs[rs1] |+| imm
                this.UpdatePC()
            | _ -> raise (IllegalInstruction(inst))

        | 0x33UL ->
            let shamt = ((__regs[rs2] &&& 0x3FUL) |> uint64) |> uint32

            match (funct3, funct7) with
            | (0x0UL, 0x00UL) ->
                printfn "执行add"
                __regs[rd] <- __regs[rs1] |+| __regs[rs2]
                this.UpdatePC()
            | _ -> raise (IllegalInstruction(inst))

        | _ -> raise (IllegalInstruction(inst))
