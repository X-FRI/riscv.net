module RISCV.NET.Core.Dram

open System

type Dram(code: inref<uint8[]>) =
    let dram: uint8[] = Array.zeroCreate<uint8> (Dram.SIZE |> int)
    do Array.blit code 0 dram 0 code.Length

    /// 初始内存大小为 128MB
    static member public SIZE: UInt64 = 1024UL * 1024UL * 128UL

    static member public BASE: UInt64 = 0x8000_0000UL

    static member public END: UInt64 = Dram.SIZE + Dram.BASE - 1UL

    member public this.Load (address: UInt64) (size: UInt64) : UInt64 =
        match size with
        | 8UL
        | 16UL
        | 32UL
        | 64UL ->
            let nbytes = size / 8UL
            let index = address - Dram.BASE |> int
            let mutable code = dram[index] |> uint64

            for i = 1 to (nbytes - 1UL) |> int do
                code <- code ||| (dram[index + i |> int] |> uint64) <<< (i * 8)

            code
        | _ -> failwith $"Load access fault %07X{address}"

    member public this.Store (address: UInt64) (size: UInt64) (value: UInt64) : Unit =
        match size with
        | 8UL
        | 16UL
        | 32UL
        | 64UL ->
            let nbytes = size / 8UL
            let index = address - Dram.BASE |> int

            for i = 1 to (nbytes - 1UL) |> int do
                dram[index + i] <- ((value |> int) >>> (8 * i)) &&& 0xFF |> uint8

        | _ -> failwith $"Store AMO access fault %07X{address}"
