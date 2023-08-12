module RiscV.Net.Dram

let BASE: uint64 = 0x8000_0000UL
let SIZE: uint64 = 1024UL * 1024UL * 128UL
let END: uint64 = SIZE + BASE - 1UL

type t = { code: uint8 array; mem: uint8 array }

let init code =
    let mem = Array.zeroCreate<uint8> (SIZE |> int)
    Array.blit code 0 mem 0 code.Length
    { code = code; mem = mem }

let load dram addr size =
    match size with
    | 8UL
    | 16UL
    | 32UL
    | 64UL ->
        let nbytes = size / 8UL |> int
        let index = (addr - BASE) |> int
        let code = ref (dram.mem[index] |> uint64)

        for i = 1 to nbytes do
            code.Value <- code.Value ||| ((dram.mem[index + (i |> int)] |> uint64) <<< (i * 8))

        Ok(code.Value)
    | _ -> Error(Error.LoadAccessFault addr)

let store dram addr size value =
    match size with
    | 8UL
    | 16UL
    | 32UL
    | 64UL ->
        let nbytes = size / 8UL |> int
        let index = (addr - BASE) |> int

        for i = 1 to nbytes do
            dram.mem[index + i] <- ((value >>> (8 * i)) &&& 0xffUL) |> uint8

        Ok()
    | _ -> Error(Error.StoreAMOAccessFault addr)
