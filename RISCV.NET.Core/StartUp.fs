module RISCV.NET.Core.StartUp

open RISCV.NET.Core.CPU
open RISCV.NET.Core.Logger
open RISCV.NET.Core.Instructions

type CPU with
    member this.StartUp() =
        Log.Info "Startup..."

        while this.PC < (this.Dram.Length |> uint64) do
            let instruction = this.Fetch()
            Log.Info $"Fetching instruction: 0x%07X{instruction}"
            this.Execute instruction
            this.PC <- this.PC + 4UL

        Log.Info "End..."
        this.DumpRegisters()
