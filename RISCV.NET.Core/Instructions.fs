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
        | 0x1u -> instruction |> this.Lh
        | 0x2u -> instruction |> this.Lw
        | 0x3u -> instruction |> this.Ld
        | 0x4u -> instruction |> this.Lbu
        | 0x5u -> instruction |> this.Lhu
        | 0x6u -> instruction |> this.Lwu
        | _ -> failwith "unreachable"

      | 0x13u ->
        let instruction = InstructionType.I.Decode instruction

        match instruction.funct3 with
        | 0x0u -> instruction |> this.Addi
        | 0x1u -> instruction |> this.Slli
        | 0x5u ->
          // 判断是SRLI还是SRAI
          if (instruction.imm &&& 0x400UL) = 0UL then
            instruction |> this.Srli
          else
            instruction |> this.Srai
        | _ ->
          failwith $"Invalid funct3 0x%02X{instruction.funct3} for opcode 0x13"

      | 0x1Bu ->
        let instruction = InstructionType.I.Decode instruction

        match instruction.funct3 with
        | 0x0u -> instruction |> this.Addiw
        | 0x1u -> instruction |> this.Slliw
        | 0x5u ->
          // 判断是SRLIW还是SRAIW
          if (instruction.imm &&& 0x400UL) = 0UL then
            instruction |> this.Srliw
          else
            instruction |> this.Sraiw
        | _ ->
          failwith $"Invalid funct3 0x%02X{instruction.funct3} for opcode 0x1B"

      | 0x33u ->
        match r_instruction.funct3, r_instruction.funct7 with
        | 0x0u, 0x00u -> r_instruction |> this.Add
        | 0x0u, 0x20u -> failwith "SUB not implemented" // 待实现
        | _ -> failwith $"Invalid funct3/funct7 combination for opcode 0x33"

      | 0x3Bu ->
        match r_instruction.funct3, r_instruction.funct7 with
        | 0x0u, 0x00u -> r_instruction |> this.Addw
        | 0x0u, 0x20u -> r_instruction |> this.Subw
        | 0x1u, 0x00u -> r_instruction |> this.Sllw
        | 0x5u, 0x00u -> r_instruction |> this.Srlw
        | 0x5u, 0x20u -> r_instruction |> this.Sraw
        | _ -> failwith $"Invalid funct3/funct7 combination for opcode 0x3B"

      | 0x23u ->
        let s_instruction = InstructionType.S.Decode instruction

        match s_instruction.funct3 with
        | 0x0u -> s_instruction |> this.Sb
        | 0x3u -> s_instruction |> this.Sd
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

  member private this.Lbu (instruction : InstructionType.I) =
    Log.Info
      $"lbu x{instruction.rd}, 0x%04X{instruction.imm}(x{instruction.rs1})"

    this.Registers[instruction.rd] <-
      (this.Load (this.Registers[instruction.rs1] + instruction.imm) 8UL)

  member private this.Lh (instruction : InstructionType.I) =
    Log.Info
      $"lh x{instruction.rd}, 0x%04X{instruction.imm}(x{instruction.rs1})"

    this.Registers[instruction.rd] <-
      (this.Load (this.Registers[instruction.rs1] + instruction.imm) 16UL)
      |> uint64

  member private this.Lhu (instruction : InstructionType.I) =
    Log.Info
      $"lhu x{instruction.rd}, 0x%04X{instruction.imm}(x{instruction.rs1})"

    this.Registers[instruction.rd] <-
      (this.Load (this.Registers[instruction.rs1] + instruction.imm) 16UL)

  member private this.Lw (instruction : InstructionType.I) =
    Log.Info
      $"lw x{instruction.rd}, 0x%04X{instruction.imm}(x{instruction.rs1})"

    this.Registers[instruction.rd] <-
      (this.Load (this.Registers[instruction.rs1] + instruction.imm) 32UL)
      |> uint64

  member private this.Lwu (instruction : InstructionType.I) =
    Log.Info
      $"lwu x{instruction.rd}, 0x%04X{instruction.imm}(x{instruction.rs1})"

    this.Registers[instruction.rd] <-
      (this.Load (this.Registers[instruction.rs1] + instruction.imm) 32UL)

  member private this.Ld (instruction : InstructionType.I) =
    Log.Info
      $"ld x{instruction.rd}, 0x%04X{instruction.imm}(x{instruction.rs1})"

    this.Registers[instruction.rd] <-
      this.Load (this.Registers[instruction.rs1] + instruction.imm) 64UL

  member private this.Sb (instruction : InstructionType.S) =
    Log.Info
      $"sb x{instruction.rs2}, 0x%04X{instruction.imm}(x{instruction.rs1})"

    this.Store
      (this.Registers[instruction.rs1] + instruction.imm)
      8UL
      this.Registers[instruction.rs2]

  member private this.Slli (instruction : InstructionType.I) =
    let shamt = instruction.imm &&& 0x3FUL
    Log.Info $"slli x{instruction.rd}, x{instruction.rs1}, {shamt}"

    this.Registers[instruction.rd] <-
      this.Registers[instruction.rs1] <<< int shamt

  member private this.Srli (instruction : InstructionType.I) =
    let shamt = instruction.imm &&& 0x3FUL
    Log.Info $"srli x{instruction.rd}, x{instruction.rs1}, {shamt}"

    this.Registers[instruction.rd] <-
      this.Registers[instruction.rs1] >>> int shamt

  member private this.Srai (instruction : InstructionType.I) =
    let shamt = instruction.imm &&& 0x3FUL
    Log.Info $"srai x{instruction.rd}, x{instruction.rs1}, {shamt}"

    // need to treat the value as a signed number for arithmetic right shift
    let value = this.Registers[instruction.rs1]

    let signExtended =
      if (value &&& 0x8000_0000_0000_0000UL) <> 0UL then
        let shifted = value >>> int shamt
        // ensure the high bit is still 1
        let mask = ~~~((1UL <<< (64 - int shamt)) - 1UL)
        shifted ||| mask
      else
        value >>> int shamt

    this.Registers[instruction.rd] <- signExtended

  member private this.Addiw (instruction : InstructionType.I) =
    Log.Info
      $"addiw x{instruction.rd}, x{instruction.rs1}, 0x%04X{instruction.imm}"

    // perform addition on the 32-bit word and then sign-extend to 64 bits
    let result =
      (this.Registers[instruction.rs1] + instruction.imm) &&& 0xFFFF_FFFFUL

    this.Registers[instruction.rd] <-
      if (result &&& 0x8000_0000UL) <> 0UL then
        result ||| 0xFFFF_FFFF_0000_0000UL
      else
        result

  member private this.Slliw (instruction : InstructionType.I) =
    let shamt = instruction.imm &&& 0x1FUL // for W operation, only the lower 5 bits are valid
    Log.Info $"slliw x{instruction.rd}, x{instruction.rs1}, {shamt}"

    let value = this.Registers[instruction.rs1] &&& 0xFFFF_FFFFUL
    let result = (value <<< int shamt) &&& 0xFFFF_FFFFUL

    this.Registers[instruction.rd] <-
      if (result &&& 0x8000_0000UL) <> 0UL then
        result ||| 0xFFFF_FFFF_0000_0000UL
      else
        result

  member private this.Srliw (instruction : InstructionType.I) =
    let shamt = instruction.imm &&& 0x1FUL
    Log.Info $"srliw x{instruction.rd}, x{instruction.rs1}, {shamt}"

    // 32-bit truncation, logical right shift
    let value = this.Registers[instruction.rs1] &&& 0xFFFF_FFFFUL
    let result = value >>> int shamt

    // sign-extend to 64 bits
    this.Registers[instruction.rd] <-
      if (result &&& 0x8000_0000UL) <> 0UL then
        result ||| 0xFFFF_FFFF_0000_0000UL
      else
        result

  member private this.Sraiw (instruction : InstructionType.I) =
    let shamt = instruction.imm &&& 0x1FUL
    Log.Info $"sraiw x{instruction.rd}, x{instruction.rs1}, {shamt}"

    // 32-bit truncation, arithmetic right shift, then sign-extend
    let value = this.Registers[instruction.rs1] &&& 0xFFFF_FFFFUL

    let signed32 =
      if (value &&& 0x8000_0000UL) <> 0UL then
        let shifted = value >>> int shamt
        // ensure the high bit is still 1 (for 32-bit)
        let mask = ~~~((1UL <<< (32 - int shamt)) - 1UL) &&& 0xFFFF_FFFFUL
        shifted ||| mask
      else
        value >>> int shamt

    // sign-extend to 64 bits
    this.Registers[instruction.rd] <-
      if (signed32 &&& 0x8000_0000UL) <> 0UL then
        signed32 ||| 0xFFFF_FFFF_0000_0000UL
      else
        signed32

  member private this.Addw (instruction : InstructionType.R) =
    Log.Info $"addw x{instruction.rd}, x{instruction.rs1}, x{instruction.rs2}"

    let value1 = this.Registers[instruction.rs1] &&& 0xFFFF_FFFFUL
    let value2 = this.Registers[instruction.rs2] &&& 0xFFFF_FFFFUL
    let result = (value1 + value2) &&& 0xFFFF_FFFFUL

    this.Registers[instruction.rd] <-
      if (result &&& 0x8000_0000UL) <> 0UL then
        result ||| 0xFFFF_FFFF_0000_0000UL
      else
        result

  member private this.Subw (instruction : InstructionType.R) =
    Log.Info $"subw x{instruction.rd}, x{instruction.rs1}, x{instruction.rs2}"

    let value1 = this.Registers[instruction.rs1] &&& 0xFFFF_FFFFUL
    let value2 = this.Registers[instruction.rs2] &&& 0xFFFF_FFFFUL
    let result = (value1 - value2) &&& 0xFFFF_FFFFUL

    this.Registers[instruction.rd] <-
      if (result &&& 0x8000_0000UL) <> 0UL then
        result ||| 0xFFFF_FFFF_0000_0000UL
      else
        result

  member private this.Sllw (instruction : InstructionType.R) =
    Log.Info $"sllw x{instruction.rd}, x{instruction.rs1}, x{instruction.rs2}"

    let value = this.Registers[instruction.rs1] &&& 0xFFFF_FFFFUL
    let shamt = int (this.Registers[instruction.rs2] &&& 0x1FUL)
    let result = (value <<< shamt) &&& 0xFFFF_FFFFUL

    this.Registers[instruction.rd] <-
      if (result &&& 0x8000_0000UL) <> 0UL then
        result ||| 0xFFFF_FFFF_0000_0000UL
      else
        result

  member private this.Srlw (instruction : InstructionType.R) =
    Log.Info $"srlw x{instruction.rd}, x{instruction.rs1}, x{instruction.rs2}"

    let value = this.Registers[instruction.rs1] &&& 0xFFFF_FFFFUL
    let shamt = int (this.Registers[instruction.rs2] &&& 0x1FUL)
    let result = value >>> shamt

    // sign-extend to 64 bits
    this.Registers[instruction.rd] <-
      if (result &&& 0x8000_0000UL) <> 0UL then
        result ||| 0xFFFF_FFFF_0000_0000UL
      else
        result

  member private this.Sraw (instruction : InstructionType.R) =
    Log.Info $"sraw x{instruction.rd}, x{instruction.rs1}, x{instruction.rs2}"

    // 32-bit truncation
    let value = this.Registers[instruction.rs1] &&& 0xFFFF_FFFFUL
    let shamt = int (this.Registers[instruction.rs2] &&& 0x1FUL)

    // sign-extend to 32-bit range
    let signed32 =
      if (value &&& 0x8000_0000UL) <> 0UL then
        let shifted = value >>> shamt
        // ensure the high bit is still 1
        let mask = ~~~((1UL <<< (32 - shamt)) - 1UL) &&& 0xFFFF_FFFFUL
        shifted ||| mask
      else
        value >>> shamt

    // sign-extend to 64 bits
    this.Registers[instruction.rd] <-
      if (signed32 &&& 0x8000_0000UL) <> 0UL then
        signed32 ||| 0xFFFF_FFFF_0000_0000UL
      else
        signed32

  member private this.Sd (instruction : InstructionType.S) =
    Log.Info
      $"sd x{instruction.rs2}, 0x%04X{instruction.imm}(x{instruction.rs1})"

    let address = this.Registers[instruction.rs1] + instruction.imm
    let value = this.Registers[instruction.rs2]
    this.Bus.Store address 64UL value
