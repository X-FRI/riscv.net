module RISCV.NET.Core.CPU

open System
open RISCV.NET.Core.Bus
open RISCV.NET.Core.Dram

type CPU(dram: uint8[]) =

    /// 32 个 64 位的通用整数寄存器
    let registers: UInt64[] = Array.create<UInt64> 32 0UL
    let bus: Bus = Bus &dram

    /// 一个 64 位的 PC, 在初始化时置 0
    let mutable pc: UInt64 = 0UL

    do
        // 将栈指针 (SP) 指向栈顶（内存的最高地址）.
        registers[2] <- Dram.END

    member public this.Dram = &dram
    member public this.Registers = &registers
    member public this.PC = &pc

    member inline public this.UpdatePC() : UInt64 = this.PC + 4UL

    member public this.Fetch() : UInt32 =
        let index = pc |> int

        // 读取 [pc; pc+1; pc+2; pc+3] 四个地址上的值并组合成一个 32 位的指令
        dram[index] |> uint32
        ||| ((dram[index + 1] |> uint32) <<< 8)
        ||| ((dram[index + 2] |> uint32) <<< 16)
        ||| ((dram[index + 3] |> uint32) <<< 24)

    member public this.DumpRegisters() : Unit =
        [ [ "zero"
            $"0x%07X{registers[0]}"
            "ra"
            $"0x%07X{registers[1]}"
            "sp"
            $"0x%07X{registers[2]}"
            "gp"
            $"0x%07X{registers[3]}" ]
          [ "tp"
            $"0x%07X{registers[4]}"
            "t0"
            $"0x%07X{registers[5]}"
            "t1"
            $"0x%07X{registers[6]}"
            "t2"
            $"0x%07X{registers[7]}" ]
          [ "s0"
            $"0x%07X{registers[8]}"
            "s1"
            $"0x%07X{registers[9]}"
            "a0"
            $"0x%07X{registers[10]}"
            "a1"
            $"0x%07X{registers[11]}" ]
          [ "a2"
            $"0x%07X{registers[12]}"
            "a3"
            $"0x%07X{registers[13]}"
            "a4"
            $"0x%07X{registers[14]}"
            "a5"
            $"0x%07X{registers[15]}" ]
          [ "a6"
            $"0x%07X{registers[16]}"
            "a7"
            $"0x%07X{registers[17]}"
            "s2"
            $"0x%07X{registers[18]}"
            "s3"
            $"0x%07X{registers[19]}" ]
          [ "s4"
            $"0x%07X{registers[20]}"
            "s5"
            $"0x%07X{registers[21]}"
            "s6"
            $"0x%07X{registers[22]}"
            "s7"
            $"0x%07X{registers[23]}" ]
          [ "s8"
            $"0x%07X{registers[24]}"
            "s9"
            $"0x%07X{registers[25]}"
            "s10"
            $"0x%07X{registers[26]}"
            "s11"
            $"0x%07X{registers[27]}" ]
          [ "t3"
            $"0x%07X{registers[28]}"
            "t4"
            $"0x%07X{registers[29]}"
            "t5"
            $"0x%07X{registers[30]}"
            "t6"
            $"0x%07X{registers[31]}" ] ]
        |> PrettyTable.prettyTable
        |> PrettyTable.withHeaders [ "REG"; "VAL"; "REG"; "VAL"; "REG"; "VAL"; "REG"; "VAL" ]
        |> PrettyTable.printTable
