module RiscV.Net.Bus

open Dram

type Bus(__code: array<uint8>) =
    let __dram: Dram = Dram(__code)

    member public this.Load(addr: uint64, size: uint64) =
        if addr >= Dram.BASE && addr <= Dram.END then
            __dram.Load(addr, size)
        else
            failwith $"Load access fault: {addr}"

    member public this.Store(addr: uint64, size: uint64, value: uint64) =
        if addr >= Dram.BASE && addr <= Dram.END then
            __dram.Store(addr, size, value)
        else
            failwith $"Store AMO access fault: {addr}"