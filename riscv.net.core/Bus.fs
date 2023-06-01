module riscv.net.core.Bus

open DRAM
open Param
open Exception

type Bus(code: array<uint8>) =
    let __dram: DRAM = DRAM(code)

    member public _.Load(addr: uint64, size: uint64) : uint64 =
        if addr >= DRAM_BASE && addr <= DRAM_END then
            __dram.Load(addr, size)
        else
            raise (LoadAccessFault(addr))

    member public _.Store(addr: uint64, size: uint64, value: uint64) : unit =
        if addr >= DRAM_BASE && addr <= DRAM_END then
            __dram.Store(addr, size, value)
        else
            raise (StoreAMOAccessFault(addr))
