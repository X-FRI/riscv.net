module RISCV.NET.Core.Tests.Addi

open NUnit.Framework
open RISCV.NET.Core.CPU
open RISCV.NET.Core.Tests.GenerateTestBinary
open RISCV.NET.Core.StartUp

[<Test>]
let Addi () =
    let gen = Gen("addi", [| "addi x29, x0, 5"; "addi x30, x0, 37" |])

    let cpu = CPU(gen.Code)
    cpu.StartUp()
