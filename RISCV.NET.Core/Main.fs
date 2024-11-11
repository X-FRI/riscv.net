module RISCV.NET.Core.Main

open RISCV.NET.Core.CPU

[<EntryPoint>]
let main argv =
  let cpu = CPU ([||])
  cpu.DumpRegisters ()

  0
