module RiscV.Net.Program

open CPU
open System.IO

[<EntryPoint>]
let main args =
    let cpu = CPU(File.ReadAllBytes(args[0]) |> Array.map uint8)
    cpu.Run()
    0
