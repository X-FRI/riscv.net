module RISCV.NET.Core.StartUp

open RISCV.NET.Core.CPU
open RISCV.NET.Core.Logger
open RISCV.NET.Core.Instructions

exception EndOfCode

type CPU with
  member this.StartUp () =
    Log.Info "---------- BEGIN ----------"

    try
      while this.PC < (this.Bus.Dram.Value.Length |> uint64) do
        match this.Fetch () with
        | 0x0UL -> raise EndOfCode
        | instruction -> this.PC <- this.Execute instruction
    with EndOfCode ->
      Log.Info "----------- END -----------"
      this.DumpRegisters ()
