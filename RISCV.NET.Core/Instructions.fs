module RISCV.NET.Core.Instructions

open System
open RISCV.NET.Core.CPU
open RISCV.NET.Core.Logger

module InstructionType =
  /// +-----------------------------------------------------------------------------------------+
  /// | funct7 (25~31) | rs2 (20~24) | rs1 (15~19) | funct3 (12~14) | rd (7~11) | opcode (0~6)  |
  /// +-----------------------------------------------------------------------------------------+
  type R =
    { opcode : UInt32
      rd : int
      rs1 : int
      rs2 : int
      funct3 : UInt32
      funct7 : UInt32 }

    static member public Decode (instruction : UInt64) : R =
      { opcode = instruction &&& 0x7FUL |> uint32
        rd = (instruction >>> 7) &&& 0x1FUL |> int
        rs1 = (instruction >>> 15) &&& 0x1FUL |> int
        rs2 = (instruction >>> 20) &&& 0x1FUL |> int
        funct3 = (instruction >>> 12) &&& 0x7UL |> uint32
        funct7 = (instruction >>> 25) &&& 0x7FUL |> uint32 }

  // +------------------------------------------------------------------------+
  // | imm (20~31) | rs1 (15~19) | funct3 (12~14) | rd (7~11) | opcode (0~6)  |
  // +------------------------------------------------------------------------+
  type I =
    { opcode : UInt32
      rd : int
      rs1 : int
      imm : UInt64
      funct3 : UInt32 }

    static member public Decode (instruction : UInt64) : I =
      { opcode = instruction &&& 0x7FUL |> uint32
        rd = (instruction >>> 7) &&& 0x1FUL |> int
        rs1 = (instruction >>> 15) &&& 0x1FUL |> int
        imm = (instruction &&& 0xFFF0_0000UL) |> int64 >>> 20 |> uint64
        funct3 = (instruction >>> 12) &&& 0x7UL |> uint32 }

  // +---------------------------------------------------------------------------------------+
  // | imm2 (25~31) | rs2 (19~24) | rs1 (15~19) | func3 (12~14) | imm1 (7~11) | opcode (0~6) |
  // +---------------------------------------------------------------------------------------+
  type S =
    { opcode : UInt32
      imm : UInt64
      funct3 : UInt32
      rs1 : int
      rs2 : int }

    static member public Decode (instruction : UInt64) : S =
      { opcode = instruction &&& 0x7FUL |> uint32
        imm =
          (((instruction &&& 0xFE000000UL) |> int64 >>> 20) |> uint64)
          ||| ((instruction >>> 7) &&& 0x1FUL)
        funct3 = (instruction >>> 12) &&& 0x7UL |> uint32
        rs1 = (instruction >>> 15) &&& 0x1FUL |> int
        rs2 = (instruction >>> 20) &&& 0x1FUL |> int }

type CPU with
  member public this.Execute (instruction : UInt64) : UInt64 =
    let r_instruction = InstructionType.R.Decode instruction

    begin
      match r_instruction.opcode with
      | 0x03u ->
        let instruction = InstructionType.I.Decode instruction

        match instruction.funct3 with
        | 0x0u -> instruction |> this.Lb
        | 0x3u -> instruction |> this.Ld
        | func3 -> failwith $"Invalid func3 0x%04X{func3}"

      | 0x13u -> InstructionType.I.Decode instruction |> this.Addi
      | 0x33u -> r_instruction |> this.Add
      | 0x23u ->
        let s_instruction = InstructionType.S.Decode instruction

        match s_instruction.funct3 with
        | 0x0u -> s_instruction |> this.Sb
        | _ -> failwith "unreachable"
      | invalid_opcode -> failwith $"Invalid opcode 0x%04X{invalid_opcode}"
    end

    this.UpdatePC ()

  member private this.Addi (instruction : InstructionType.I) =
    Log.Info
      $"addi x{instruction.rd}, x{instruction.rs1}, 0x%04X{instruction.imm}"

    this.Registers[instruction.rd] <-
      this.Registers[instruction.rs1] + instruction.imm

  member private this.Add (instruction : InstructionType.R) =
    Log.Info $"add x{instruction.rd}, x{instruction.rs1}, x{instruction.rs2}"

    this.Registers[instruction.rd] <-
      this.Registers[instruction.rs1] + this.Registers[instruction.rs2]

  member private this.Lb (instruction : InstructionType.I) =
    Log.Info
      $"lb x{instruction.rd}, 0x%04X{instruction.imm}(x{instruction.rs1})"

    this.Registers[instruction.rd] <-
      (this.Load (this.Registers[instruction.rs1] + instruction.imm) 8UL)
      |> uint64

  member private this.Ld (instruction : InstructionType.I) =
    Log.Info
      $"ld x{instruction.rd}, 0x%04X{instruction.imm}(x{instruction.rs1})"

    this.Registers[instruction.rd] <-
      this.Load (this.Registers[instruction.rs1] + instruction.imm) 64UL
      |> uint64

  member private this.Sb (instruction : InstructionType.S) =
    Log.Info
      $"sb x{instruction.rs2}, 0x%04X{instruction.imm}(x{instruction.rs1})"

    this.Store
      (this.Registers[instruction.rs1] + instruction.imm)
      8UL
      this.Registers[instruction.rs2]
