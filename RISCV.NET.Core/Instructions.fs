module RISCV.NET.Core.Instructions

open System
open RISCV.NET.Core.CPU

module InstructionType =
  /// +-----------------------------------------------------------------------------------------+
  /// | funct7 (25~31) | rs2 (20~24) | rs1 (15~19) | funct3 (12~14) | rd (7~11) | opcode (0~6)  |
  /// +-----------------------------------------------------------------------------------------+
  type R =
    { opcode: UInt32
      rd: int
      rs1: int
      rs2: int
      funct3: UInt32
      funct7: UInt32 }

    static member public Decode (instruction: UInt32) =
      { opcode = instruction &&& 0x7Fu
        rd = (instruction >>> 7) &&& 0x1Fu |> int
        rs1 = (instruction >>> 15) &&& 0x1Fu |> int
        rs2 = (instruction >>> 20) &&& 0x1Fu |> int
        funct3 = (instruction >>> 12) &&& 0x7u
        funct7 = (instruction >>> 25) &&& 0x7Fu }

  // +------------------------------------------------------------------------+
  // | imm (20~31) | rs1 (15~19) | funct3 (12~14) | rd (7~11) | opcode (0~6)  |
  // +------------------------------------------------------------------------+
  type I =
    { opcode: UInt32
      rd: int
      rs1: int
      imm: UInt64
      funct3: UInt32 }

    static member public Decode (instruction: UInt32) =
      { opcode = instruction &&& 0x7Fu
        rd = (instruction >>> 7) &&& 0x1Fu |> int
        rs1 = (instruction >>> 15) &&& 0x1Fu |> int
        imm = (instruction &&& 0xFFF0_0000u) |> int64 >>> 20 |> uint64
        funct3 = (instruction >>> 12) &&& 0x7u }

type CPU with
  member public this.Execute (instruction: UInt32) =
    let r_instruction = InstructionType.R.Decode instruction
    let i_instruction = InstructionType.I.Decode instruction

    match r_instruction.opcode with
    | 0x13u -> this.Addi i_instruction
    | 0x33u -> this.Add r_instruction
    | invalid_opcode -> failwith $"Invalid opcode {invalid_opcode}"

  member private this.Addi (instruction: InstructionType.I) =
    this.Registers[instruction.rd] <-
      this.Registers[instruction.rs1] + instruction.imm

  member private this.Add (instruction: InstructionType.R) =
    this.Registers[instruction.rd] <-
      this.Registers[instruction.rs1] + this.Registers[instruction.rs2]
