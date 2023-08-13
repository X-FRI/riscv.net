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
    { regs : Regs.t
      bus : Bus.t
      mutable pc : uint64 }

type pc_update_state =
    | Auto of unit
    | Custom of uint64

let init code =
    let regs = Regs.init ()
    regs[2] <- Dram.END

    { regs = regs
      bus = Bus.init (Dram.init code)
      pc = Dram.BASE }

let fetch cpu = Bus.load cpu.bus cpu.pc 32UL

let update_pc cpu pc_update_state =
    match pc_update_state with
    | Auto() -> cpu.pc <- cpu.pc + 4UL
    | Custom new_pc -> cpu.pc <- new_pc

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
            Result.map
                (fun value -> Auto(cpu.regs[rd] <- (value |> int8 |> int64 |> uint64)))
                (Bus.load cpu.bus addr 8UL)
        // lh
        | 0x1UL ->
            Result.map
                (fun value -> Auto(cpu.regs[rd] <- (value |> int8 |> int64 |> uint64)))
                (Bus.load cpu.bus addr 16UL)
        // lw
        | 0x2UL ->
            Result.map
                (fun value -> Auto(cpu.regs[rd] <- (value |> int8 |> int64 |> uint64)))
                (Bus.load cpu.bus addr 32UL)
        // ld
        | 0x3UL ->
            Result.map
                (fun value -> Auto(cpu.regs[rd] <- (value |> int8 |> int64 |> uint64)))
                (Bus.load cpu.bus addr 64UL)
        // lbu
        | 0x4UL ->
            Result.map
                (fun value -> Auto(cpu.regs[rd] <- (value |> int8 |> int64 |> uint64)))
                (Bus.load cpu.bus addr 8UL)
        // lhu
        | 0x5UL ->
            Result.map
                (fun value -> Auto(cpu.regs[rd] <- (value |> int8 |> int64 |> uint64)))
                (Bus.load cpu.bus addr 16UL)
        // lwu
        | 0x6UL ->
            Result.map
                (fun value -> Auto(cpu.regs[rd] <- (value |> int8 |> int64 |> uint64)))
                (Bus.load cpu.bus addr 32UL)

        | _ -> Error(Error.IllegalInstruction inst)

    | 0x13UL ->
        let imm = ((inst &&& 0xfff00000UL) |> int32 |> int64 >>> 20) |> uint64
        let shamt = (imm &&& 0x3fUL) |> uint32

        match funct3 with
        // addi
        | 0x0UL -> Ok(Auto(cpu.regs[rd] <- (cpu.regs[rs1] + imm)))
        // slli
        | 0x1UL -> Ok(Auto(cpu.regs[rd] <- (cpu.regs[rs1] <<< (shamt |> int32))))
        // slti
        | 0x2UL ->
            Ok(
                Auto(
                    cpu.regs[rd] <-
                        if (cpu.regs[rs1] |> int64) < (imm |> int64) then
                            1UL
                        else
                            0UL
                )
            )
        // sltiu
        | 0x3UL -> Ok(Auto(cpu.regs[rd] <- if cpu.regs[rs1] < imm then 1UL else 0UL))
        // xori
        | 0x4UL -> Ok(Auto(cpu.regs[rd] <- (cpu.regs[rs1] ^^^ imm)))
        | 0x5UL ->
            match funct7 >>> 1 with
            // srli
            | 0x00UL -> Ok(Auto(cpu.regs[rd] <- (cpu.regs[rs1] >>> (shamt |> int32))))
            // srai
            | 0x10UL ->
                Ok(
                    Auto(
                        cpu.regs[rd] <-
                            (((cpu.regs[rs1] |> int64) >>> (shamt |> int32) |> uint64))
                    )
                )
            | _ -> Error(Error.IllegalInstruction inst)

        // ori
        | 0x6UL -> Ok(Auto(cpu.regs[rd] <- (cpu.regs[rs1] ||| imm)))
        // andi
        | 0x7UL -> Ok(Auto(cpu.regs[rd] <- (cpu.regs[rs1] &&& imm)))
        | _ -> Error(Error.IllegalInstruction inst)

    // auipc
    | 0x17UL ->
        let imm = inst &&& 0xfffff000UL
        Ok(Auto(cpu.regs[rd] <- (cpu.pc + imm)))

    | 0x1bUL ->
        let imm = ((inst |> int32 |> int64) >>> 20) |> uint64
        let shamt = (imm &&& 0x1fUL) |> uint32

        match funct3 with
        // addiw
        | 0x0UL ->
            Ok(Auto(cpu.regs[rd] <- ((cpu.regs[rs1] + imm) |> int32 |> int64 |> uint64)))
        // slliw
        | 0x1UL ->
            Ok(
                Auto(
                    cpu.regs[rd] <-
                        ((cpu.regs[rs1] >>> (shamt |> int32)) |> int32 |> int64 |> uint64)
                )
            )
        | 0x5UL ->
            match funct7 with
            // srliw
            | 0x00UL ->
                Ok(
                    Auto(
                        cpu.regs[rd] <-
                            (((cpu.regs[rs1] |> uint32) >>> (shamt |> int32))
                             |> int32
                             |> int64
                             |> uint64)
                    )
                )
            // sraiw
            | 0x20UL ->
                Ok(
                    Auto(
                        cpu.regs[rd] <-
                            (((cpu.regs[rs1] |> int32) >>> (shamt |> int32))
                             |> int64
                             |> uint64)
                    )
                )
            | _ -> Error(Error.IllegalInstruction inst)
        | _ -> Error(Error.IllegalInstruction inst)
    | 0x23UL ->
        let imm =
            (((inst &&& 0xfe000000UL) |> int32 |> int64 >>> 20) |> uint64)
            ||| ((inst >>> 7) &&& 0x1fUL)

        let addr = cpu.regs[rs1] + imm

        match funct3 with
        | 0x0UL ->
            Result.map (fun () -> Auto()) (Bus.store cpu.bus addr 8UL cpu.regs[rs2])
        | 0x1UL ->
            Result.map (fun () -> Auto()) (Bus.store cpu.bus addr 16UL cpu.regs[rs2])
        | 0x2UL ->
            Result.map (fun () -> Auto()) (Bus.store cpu.bus addr 32UL cpu.regs[rs2])
        | 0x3UL ->
            Result.map (fun () -> Auto()) (Bus.store cpu.bus addr 64UL cpu.regs[rs2])
        | _ -> failwith "Unreachable!!!!!!!"
    | 0x33UL ->
        let shamt = ((cpu.regs[rs2] &&& 0x3fUL) |> uint64) |> uint32

        match (funct3, funct7) with
        // add
        | (0x0UL, 0x00UL) -> Ok(Auto(cpu.regs[rd] <- (cpu.regs[rs1] + cpu.regs[rs2])))
        // mul
        | (0x0UL, 0x01UL) -> Ok(Auto(cpu.regs[rd] <- (cpu.regs[rs1] + cpu.regs[rs2])))
        // sub
        | (0x0UL, 0x20UL) -> Ok(Auto(cpu.regs[rd] <- (cpu.regs[rs1] + cpu.regs[rs2])))
        // sll
        | (0x1UL, 0x00UL) -> Ok(Auto(cpu.regs[rd] <- (cpu.regs[rs1] + (shamt |> uint64))))
        // slt
        | (0x2UL, 0x00UL) ->
            Ok(
                Auto(
                    cpu.regs[rd] <-
                        (if (cpu.regs[rs1] |> int64) < (cpu.regs[rs2] |> int64) then
                             1UL
                         else
                             0UL)
                )
            )
        // sltu
        | (0x3UL, 0x00UL) ->
            Ok(Auto(cpu.regs[rd] <- (if cpu.regs[rs1] < cpu.regs[rs2] then 1UL else 0UL)))
        // xor
        | (0x4UL, 0x00UL) -> Ok(Auto(cpu.regs[rd] <- (cpu.regs[rs1] ^^^ cpu.regs[rs2])))
        // srl
        | (0x5UL, 0x00UL) ->
            Ok(Auto(cpu.regs[rd] <- (cpu.regs[rs1] >>> (shamt |> int32))))
        // sra
        | (0x5UL, 0x20UL) ->
            Ok(
                Auto(
                    cpu.regs[rd] <-
                        (((cpu.regs[rs1] |> int64) >>> (shamt |> int32)) |> uint64)
                )
            )
        // or
        | (0x6UL, 0x00UL) -> Ok(Auto(cpu.regs[rd] <- (cpu.regs[rs1] ||| cpu.regs[rs2])))
        // and
        | (0x7UL, 0x00UL) -> Ok(Auto(cpu.regs[rd] <- (cpu.regs[rs1] &&& cpu.regs[rs2])))
        | _ -> Error(Error.IllegalInstruction inst)
    // lui
    | 0x37UL ->
        Ok(Auto(cpu.regs[rd] <- ((inst &&& 0xfffff000UL) |> int32 |> int64 |> uint64)))
    | 0x3bUL ->
        let shamt = (cpu.regs[rs2] &&& 0x1fUL) |> uint32

        match (funct3, funct7) with
        // addw
        | (0x0UL, 0x00UL) ->
            Ok(
                Auto(
                    cpu.regs[rd] <-
                        ((cpu.regs[rs1] + (cpu.regs[rs2])) |> int32 |> int64 |> uint64)
                )
            )
        // subw
        | (0x0UL, 0x20UL) ->
            Ok(
                Auto(
                    cpu.regs[rd] <-
                        (((cpu.regs[rs1] - (cpu.regs[rs2])) |> int32) |> uint64)
                )
            )
        // sllw
        | (0x1UL, 0x00UL) ->
            Ok(
                Auto(
                    cpu.regs[rd] <-
                        ((((cpu.regs[rs1] |> uint32) <<< (shamt |> int32))
                          |> int32
                          |> uint64))
                )
            )
        // srlw
        | (0x5UL, 0x00UL) ->
            Ok(
                Auto(
                    cpu.regs[rd] <-
                        (((cpu.regs[rs1] |> uint32) >>> (shamt |> int32))
                         |> int32
                         |> uint64)
                )
            )
        // sraw
        | (0x5UL, 0x20UL) ->
            Ok(
                Auto(
                    cpu.regs[rd] <-
                        ((cpu.regs[rs1] |> int32) >>> (shamt |> int32)) |> uint64
                )
            )
        | _ -> Error(Error.IllegalInstruction inst)
    | 0x63UL ->
        let imm =
            (((inst &&& 0x80000000UL) |> int32 |> int64 >>> 19) |> uint64)
            ||| ((inst &&& 0x80UL) <<< 4)
            ||| ((inst >>> 20) &&& 0x7e0UL)
            ||| ((inst >>> 7) &&& 0x1eUL)

        match funct3 with
        // beq
        | 0x0UL ->
            if cpu.regs[rs1] = cpu.regs[rs2] then
                Ok(Custom(cpu.pc + imm))
            else
                failwith "Unreachable!!!!!!!"
        // bne
        | 0x1UL ->
            if cpu.regs[rs1] <> cpu.regs[rs2] then
                Ok(Custom(cpu.pc + imm))
            else
                failwith "Unreachable!!!!!!!"
        // blt
        | 0x4UL ->
            if (cpu.regs[rs1] |> int64) < (cpu.regs[rs2] |> int64) then
                Ok(Custom(cpu.pc + imm))
            else
                failwith "Unreachable!!!!!!!"
        // bge
        | 0x5UL ->
            if (cpu.regs[rs1] |> int64) >= (cpu.regs[rs2] |> int64) then
                Ok(Custom(cpu.pc + imm))
            else
                failwith "Unreachable!!!!!!!"
        // bltu
        | 0x6UL ->
            if cpu.regs[rs1] < cpu.regs[rs2] then
                Ok(Custom(cpu.pc + imm))
            else
                failwith "Unreachable!!!!!!!"
        // bgeu
        | 0x7UL ->
            if cpu.regs[rs1] >= cpu.regs[rs2] then
                Ok(Custom(cpu.pc + imm))
            else
                failwith "Unreachable!!!!!!!"
        | _ -> Error(Error.IllegalInstruction inst)
    // jalr
    | 0x67UL ->
        cpu.regs[rd] <- (cpu.pc + 4UL)
        let imm = (((inst &&& 0xfff00000UL) |> int32 |> int64) >>> 20) |> uint64
        let new_pc = (cpu.regs[rs1] + imm) &&& ((~~~ 1) |> uint64)

        Ok(Custom(new_pc))

    // jal
    | 0x6fUL ->
        cpu.regs[rd] <- (cpu.pc + 4UL)

        let imm =
            (((inst &&& 0x80000000UL) |> int32 |> int64 >>> 11) |> uint64)
            ||| (inst &&& 0xff000UL)
            ||| ((inst >>> 9) &&& 0x800UL)
            ||| ((inst >>> 20) &&& 0x7feUL)

        Ok(Custom(cpu.pc + imm))

    | _ -> Error(Error.IllegalInstruction inst)

    |> Result.map (fun state -> update_pc cpu state)


let dump_regs cpu =
    printfn $"o- Registers"
    printfn $"o- pc = %X{cpu.pc}"

    cpu.regs[0] <- 0UL

    for i in 0..4..31 do
        printfn
            $"x{i}({Regs.RVABI[i]}) = 0x%0x{cpu.regs[i]}\tx{i + 1}({Regs.RVABI[i + 1]}) = 0x%0x{cpu.regs[i + 1]}\tx{i + 2}({Regs.RVABI[i + 2]}) = 0x%0x{cpu.regs[i + 2]}\tx{i + 3}({Regs.RVABI[i + 3]}) = 0x%0x{cpu.regs[i + 3]}"

let rec run cpu =
    match fetch cpu |> Result.bind (fun inst -> execute cpu inst) with
    | Ok() -> run cpu
    | err -> err
