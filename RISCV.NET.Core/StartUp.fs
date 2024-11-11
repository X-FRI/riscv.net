module RISCV.NET.Core.StartUp

open RISCV.NET.Core.CPU
open RISCV.NET.Core.Logger
open RISCV.NET.Core.Instructions

type CPU with
    member this.StartUp() =
        Log.Info "---------- BEGIN ----------"

        while this.PC < (this.Dram.Length |> uint64) do
            let instruction = this.Fetch()
            Log.Info $"Fetching: 0x%07X{instruction}"
            this.PC <- this.Execute instruction

        Log.Info "----------- END -----------"
        this.DumpRegisters()
