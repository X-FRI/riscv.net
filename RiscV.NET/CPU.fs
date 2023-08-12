module RiscV.Net.CPU

module Regs =
    type t = array<uint64>
    let init () = Array.zeroCreate<uint64> (32)

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

type t =
    { regs: Regs.t
      bus: Bus.t
      mutable pc: uint64 }

let init code =
    let regs = Regs.init ()
    regs[2] <- Dram.END

    { regs = regs
      bus = Bus.init (Dram.init code)
      pc = Dram.BASE }

let fetch cpu = Bus.load cpu.bus cpu.pc 32UL
let update_pc cpu = cpu.pc <- cpu.pc + 4UL

let execute cpu inst =
    let opcode = inst &&& 0x0000007fUL
    let rd = ((inst &&& 0x00000f80UL) >>> 7) |> int
    let rs1 = ((inst &&& 0x000f8000UL) >>> 15) |> int
    let rs2 = ((inst &&& 0x01f00000UL) >>> 20) |> int
    let funct3 = (inst &&& 0x00007000UL) >>> 12
    let funct7 = (inst &&& 0xfe000000UL) >>> 25

    cpu.regs[0] <- 0UL

    match opcode with
    | 0x03UL ->
        let imm = ((inst |> int32 |> int64) >>> 20) |> uint64
        let addr = cpu.regs[rs1] + imm

        match funct3 with
        // lb
        | 0x0UL ->
            Result.map (fun value -> cpu.regs[rd] <- (value |> int8 |> int64 |> uint64)) (Bus.load cpu.bus addr 8UL)

        // lh
        | 0x1UL ->
            Result.map (fun value -> cpu.regs[rd] <- (value |> int8 |> int64 |> uint64)) (Bus.load cpu.bus addr 16UL)

        // lw
        | 0x2UL ->
            Result.map (fun value -> cpu.regs[rd] <- (value |> int8 |> int64 |> uint64)) (Bus.load cpu.bus addr 32UL)

        // ld
        | 0x3UL ->
            Result.map (fun value -> cpu.regs[rd] <- (value |> int8 |> int64 |> uint64)) (Bus.load cpu.bus addr 64UL)

        // lbu
        | 0x4UL ->
            Result.map (fun value -> cpu.regs[rd] <- (value |> int8 |> int64 |> uint64)) (Bus.load cpu.bus addr 8UL)

        // lhu
        | 0x5UL ->
            Result.map (fun value -> cpu.regs[rd] <- (value |> int8 |> int64 |> uint64)) (Bus.load cpu.bus addr 16UL)

        // lwu
        | 0x6UL ->
            Result.map (fun value -> cpu.regs[rd] <- (value |> int8 |> int64 |> uint64)) (Bus.load cpu.bus addr 32UL)

        | _ -> Error(Error.IllegalInstruction inst)

    | 0x13UL ->
        let imm = ((inst &&& 0xfff00000UL) |> int32 |> int64 >>> 20) |> uint64
        let shamt = (imm &&& 0x3fUL) |> uint32

        match funct3 with
        // addi
        | 0x0UL -> Ok(cpu.regs[rd] <- (cpu.regs[rs1] + imm))
        | _ -> Error(Error.IllegalInstruction inst)

    | 0x33UL ->
        // "SLL, SRL, and SRA perform logical left, logical right, and arithmetic right
        // shifts on the value in register rs1 by the shift amount held in register rs2.
        // In RV64I, only the low 6 bits of rs2 are considered for the shift amount."
        let shamt = ((cpu.regs[rs2] &&& 0x3fUL) |> uint64) |> uint32

        match (funct3, funct7) with
        // add
        | (0x0UL, 0x00UL) -> Ok(cpu.regs[rd] <- (cpu.regs[rs1] + cpu.regs[rs2]))
        | _ -> Error(Error.IllegalInstruction inst)

    | _ -> Error(Error.IllegalInstruction inst)

    |> Result.map (fun _ -> update_pc cpu)


let dump_regs cpu =
    printfn $"o- Registers"
    cpu.regs[0] <- 0UL

    for i in 0..4..31 do
        printfn
            $"x{i}({Regs.RVABI[i]}) = 0x%0x{cpu.regs[i]}\tx{i + 1}({Regs.RVABI[i + 1]}) = 0x%0x{cpu.regs[i + 1]}\tx{i + 2}({Regs.RVABI[i + 2]}) = 0x%0x{cpu.regs[i + 2]}\tx{i + 3}({Regs.RVABI[i + 3]}) = 0x%0x{cpu.regs[i + 3]}"

let rec run cpu =
    match fetch cpu |> Result.bind (fun inst -> execute cpu inst) with
    | Ok() -> run cpu
    | err -> err
