module RISCV.NET.Core.Instructions

open System

module Types =
  // +-----------------------------------------------------------------------------------------+
  // | funct7 (25~31) | rs2 (20~24) | rs1 (15~19) | funct3 (12~14) | rd (7~11) | opcode (0~6) |
  // +---------------------------------------------------------------------------------------+
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
