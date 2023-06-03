module riscv.net.core.DRAM

open System
open Param
open Exception

type DRAM (code : array<uint8>) =

    let __dram : array<uint8> = Array.zeroCreate<uint8> (int (DRAM_SIZE))
    do Array.Copy(code, __dram, code.Length)

    /// addr/size must be valid
    member private _.Check (addr : uint64, size : uint64) : unit =
        match size with
        | 8UL
        | 16UL
        | 32UL
        | 64UL -> ()
        | _ -> raise (LoadAccessFault(addr))

    member public this.Load (addr : uint64, size : uint64) : uint64 =
        this.Check(addr, size)

        let nbytes = size / 8UL in
        let index = int (addr - DRAM_BASE) in
        let mutable code = uint64 __dram[index]

        for i = 1 to (int (nbytes) - 1) do
            code <- (code ||| ((uint64 (__dram[index + i])) <<< (i * 8)))

        // printfn $"addr = {addr}, size = {size}, nbytes = {nbytes}. index = {index}, code = {code}"

        code


    member public this.Store (addr : uint64, size : uint64, value : uint64) : unit =
        this.Check(addr, size)

        let nbytes = size / 8UL in
        let index = int (addr - DRAM_BASE) in

        for i = 0 to int (nbytes) - 1 do
            __dram[index + 1] <- uint8 ((value >>> (8 * i)) &&& 0xFFUL)
