module RiscV.NET.Native.Main

open System.IO
open RiscV.NET.Core

[<EntryPoint>]
let main args =
    let cpu = CPU.init (File.ReadAllBytes(args[0]) |> Array.map uint8)

    match CPU.run cpu with
    | Ok() -> printfn "Done"
    | Error err ->
        CPU.dump_regs cpu
        failwith $"{Error.to_string err}"

    0
