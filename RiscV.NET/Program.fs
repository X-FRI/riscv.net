module RiscV.NET.Program

open CPU

[<EntryPoint>]
let main args = 
    CPU([]).DumpRegisters()
    0