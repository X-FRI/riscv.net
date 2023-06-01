module riscv.net.core.DRAM

open System
open Param
open Exception

type DRAM(code: array<uint8>) =

    let __dram: array<uint8> = Array.zeroCreate<uint8> (int (DRAM_SIZE))
    do Array.Copy(code, __dram, 0)

    member public _.Load(addr: uint64, size: uint64) : Result<uint64, Exception> =
        match size with
        | 8UL
        | 16UL
        | 32UL
        | 64UL ->
            let nbytes: uint64 = size / 8UL in
            let index: int = int (addr - DRAM_BASE) in
            let mutable code: uint64 = uint64 __dram[index]

            // shift the bytes to build up the desired value
            for i = 1 to int (nbytes) do
                code <- (code ||| ((uint64 (__dram[index + i])) <<< (i * 8)))

            Ok(code)
        | _ -> Error(LoadAccessFault(addr))

    member public _.Store(addr: uint64, size: uint64, value: uint64) : Result<unit, Exception> =
        match size with
        | 8UL
        | 16UL
        | 32UL
        | 64UL ->
            let nbytes: uint64 = size / 8UL in
            let index: int = int (addr - DRAM_BASE) in

            // shift the bytes to build up the desired value
            for i = 0 to int (nbytes) do
                __dram[index + 1] <- uint8 ((value >>> (8 * i)) &&& 0xFFUL)

            Ok(())
        | _ -> Error(StoreAMOAccessFault(addr))
