module RiscV.NET.Core.Bus

type t = { dram : Dram.t }

let init dram = { dram = dram }

let load bus addr size =
    if addr >= Dram.BASE && addr <= Dram.END then
        Dram.load bus.dram addr size
    else
        Error(Error.LoadAccessFault addr)

let store bus addr size value =
    if addr >= Dram.BASE && addr <= Dram.END then
        Dram.store bus.dram addr size value
    else
        Error(Error.StoreAMOAccessFault addr)
