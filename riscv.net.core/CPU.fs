module riscv.net.core.CPU

open riscv.net.core.Bus

module Add =
    let (|+|) (a: uint64) (b: uint64) : uint64 =
        let sum = a + b
        if sum < a || sum < b then a &&& b else sum

open Add
open System
open Param

type CPU(code: array<uint8>) as self =

    [<DefaultValue>]
    val mutable public PC: uint64

    let __regs: array<uint64> = Array.zeroCreate<uint64> (32)
    let __bus: Bus = Bus(code)

    do
        __regs[2] <- DRAM_END
        // self.PC <-  DRAM_BASE

        printfn $"{self.PC} <-----"

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

    member public _.Load(addr: uint64, size: uint64) : Result<uint64, Exception> = __bus.Load(addr, size)

    member public _.Store(addr: uint64, size: uint64, value: uint64) : Result<unit, Exception> =
        __bus.Store(addr, size, value)

    member public this.Fetch() : Result<uint64, Exception> = __bus.Load(this.PC, 32UL)

    member inline private this.UpdatePC() : Result<uint64, Exception> = Ok(this.PC + 4UL)

    member public this.Execute(instruction: uint64) : Result<uint64, Exception> =
        // decode as r-type
        let opcode = uint (instruction &&& 0x7FUL) in
        let rd = int ((instruction >>> 7) &&& 0x1FUL) in
        let rs1 = int ((instruction >>> 15) &&& 0x1FUL) in
        let rs2 = int ((instruction >>> 20) &&& 0x1FUL) in
        let _funct3 = int ((instruction >>> 12) &&& 0x7UL) in
        let _funct7 = int ((instruction >>> 25) &&& 0x7FUL) in

        // x0 is hardwired zero
        __regs[0] <- 0UL

        match opcode with
        // addi
        | 0x13u ->
            __regs[rd] <- __regs[rs1] |+| uint64 ((int64 (instruction &&& 0xFFF00000UL)) >>> 20l)
            this.UpdatePC()

        // add
        | 0x33u ->
            __regs[rd] <- __regs[rs1] |+| __regs[rs2]
            this.UpdatePC()

        | _ -> raise (Exception $"----------> Invalid opcode: 0x%X{opcode}")
