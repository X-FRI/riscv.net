module riscv.net.core.Bus

open DRAM
open Param
open Exception
open System

type Bus(code: array<uint8>) =
    let __dram: DRAM = DRAM(code)

    member public _.Load(addr: uint64, size: uint64) : Result<uint64, Exception> =
        if addr >= DRAM_BASE && addr <= DRAM_END then
            __dram.Load(addr, size)
        else
            Error(LoadAccessFault(addr))

    member public _.Store(addr: uint64, size: uint64, value: uint64) : Result<unit, Exception> =
        if addr >= DRAM_BASE && addr <= DRAM_END then
            __dram.Store(addr, size, value)
        else
            Error(StoreAMOAccessFault(addr))
