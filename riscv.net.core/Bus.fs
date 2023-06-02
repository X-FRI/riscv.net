module riscv.net.core.Bus

open DRAM
open Param
open Exception

type Bus (code : array<uint8>) =
    let __dram : DRAM = DRAM(code)
    
    /// addr must be within the effective size of the DRAM
    member private _.Check (addr : uint64) : () =
        if addr >= DRAM_BASE && addr <= DRAM_END then () else raise (LoadAccessFault(addr)) 
    
    member public this.Load (addr : uint64, size : uint64) : uint64 =
        this.Check(addr)
        __dram.Load(addr, size)
        
    member public this.Store (addr : uint64, size : uint64, value : uint64) : unit =
        this.Check(addr)
        __dram.Store(addr, size, value)