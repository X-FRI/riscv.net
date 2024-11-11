module RISCV.NET.Core.Bus

open System
open RISCV.NET.Core.Dram

type Bus(code: inref<uint8[]>) =
    let dram = Dram &code

    member public this.Load (address: UInt64) (size: UInt64) : UInt64 =
        if address >= Dram.BASE && address <= Dram.END then
            dram.Load address size
        else
            failwith $"Load access fault {address}"

    member public this.Store (address: UInt64) (size: UInt64) (value: UInt64) : Unit =
        if address >= Dram.BASE && address <= Dram.END then
            dram.Store address size value
        else
            failwith $"Store AMO access fault {address}"
