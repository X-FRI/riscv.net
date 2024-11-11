module RISCV.NET.Core.Tests.Instructions

open NUnit.Framework
open RISCV.NET.Core.CPU
open RISCV.NET.Core.Tests.GenerateTestBinary
open RISCV.NET.Core.StartUp

[<Test>]
let Addi () =
    let gen = Gen("addi", [| "addi x29, x0, 5"; "addi x30, x0, 37" |])
    let cpu = CPU(gen.Code)
    cpu.StartUp()

    Assert.That(cpu.Registers[29], Is.EqualTo 5)
    Assert.That(cpu.Registers[30], Is.EqualTo 37)

[<Test>]
let Add () =
    let gen =
        Gen("addi", [| "addi x29, x0, 5"; "addi x30, x0, 37"; "add x31, x30, x29" |])

    let cpu = CPU(gen.Code)
    cpu.StartUp()

    Assert.That(cpu.Registers[31], Is.EqualTo 42)
