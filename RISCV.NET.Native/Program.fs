module RISCV.NET.Native.Program

open System
open RISCV.NET.Core.CPU
open RISCV.NET.Core.StartUp

[<EntryPoint>]
let main args =

  let cpu = CPU (IO.File.ReadAllBytes args[0])
  cpu.StartUp ()

  0
