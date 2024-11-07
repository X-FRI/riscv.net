module RISCV.NET.Core.CPU

open System

type CPU (dram: uint8[]) as self =

  /// 32 个 64 位的通用整数寄存器
  let registers: UInt64[] = Array.create<UInt64> 32 0UL

  /// 一个 64 位的 PC, 在初始化时置 0
  let pc: UInt64 = 0UL

  do
    // 将栈指针 (SP) 指向栈顶（内存的最高地址）.
    registers[2] <- CPU.DRAM_SIZE - 1UL

  member public this.Dram = &dram

  (** 初始内存大小为 128MB *)
  static member public DRAM_SIZE: UInt64 = 1024UL * 1024UL * 128UL

  member public this.Fetch () : UInt32 =
    let index = pc |> int

    // 读取 [pc; pc+1; pc+2; pc+3] 四个地址上的值并组合成一个 32 位的指令
    dram[index] |> uint32
    ||| ((dram[index + 1] |> uint32) <<< 8)
    ||| ((dram[index + 2] |> uint32) <<< 16)
    ||| ((dram[index + 3] |> uint32) <<< 24)

  member public this.Execute (instruction: UInt32) =
    let r_instruction = Instructions.Types.R.Decode instruction
    let i_instruction = Instructions.Types.I.Decode instruction

    match r_instruction.opcode with
    | 0x13u ->
      // addi
      registers[i_instruction.rd] <-
        registers[i_instruction.rs1] + i_instruction.imm

    | 0x33u ->
      // add
      registers[r_instruction.rd] <-
        registers[r_instruction.rs1] + registers[r_instruction.rs2]
    | invalid_opcode -> failwith $"Invalid opcode {invalid_opcode}"
