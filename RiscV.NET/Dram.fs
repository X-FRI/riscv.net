module RiscV.Net.Dram

type Dram(__code: array<uint8>) =
    /// memory, a byte-array. There is no memory in real CPU.
    let __dram: array<uint8> = Array.zeroCreate<uint8> (Dram.SIZE |> int)

    do Array.blit __code 0 __dram 0 __code.Length

    static member public BASE: uint64 = 0x8000_0000UL
    static member public SIZE: uint64 = 1024UL * 1024UL * 128UL
    static member public END: uint64 = Dram.SIZE + Dram.BASE - 1UL


    /// addr/size must be valid. Check in bus
    member public this.Load(addr: uint64, size: uint64) : uint64 =
        match size with
        | 8UL
        | 16UL
        | 32UL
        | 64UL ->
            let nbytes = size / 8UL |> int
            let index = (addr - Dram.BASE) |> int
            let code = ref (__dram[index] |> uint64)

            for i = 1 to nbytes do
                code.Value <- code.Value ||| ((__dram[index + (i |> int)] |> uint64) <<< (i * 8))

            code.Value
        | _ -> failwith $"Load access fault: {addr}"

    ///  addr/size must be valid. Check in bus
    member public this.Store(addr: uint64, size: uint64, value: uint64) =
        match size with
        | 8UL
        | 16UL
        | 32UL
        | 64UL ->
            let nbytes = size / 8UL |> int
            let index = (addr - Dram.BASE) |> int

            for i = 1 to nbytes do
                __dram[index + i] <- ((value >>> (8 * i)) &&& 0xffUL) |> uint8

        | _ -> failwith $"Store AMO access fault: {addr}"
