module RISCV.NET.Core.Tests.Instructions

open NUnit.Framework
open RISCV.NET.Core.CPU
open RISCV.NET.Core.Tests.GenerateTestBinary
open RISCV.NET.Core.StartUp

[<Test>]
let Addi () =
  use gen = new Gen ("addi", [| "addi x29, x0, 5" ; "addi x30, x0, 37" |])
  let cpu = CPU (gen.Code)
  cpu.StartUp ()

  Assert.That (cpu.Registers[29], Is.EqualTo 5)
  Assert.That (cpu.Registers[30], Is.EqualTo 37)

[<Test>]
let Add () =
  use gen =
    new Gen (
      "add",
      [| "addi x29, x0, 5"
         "addi x30, x0, 37"
         "add x31, x30, x29" |]
    )

  let cpu = CPU (gen.Code)
  cpu.StartUp ()

  Assert.That (cpu.Registers[31], Is.EqualTo 42)

[<Test>]
let Sb () =
  use gen = new Gen ("sb", [| "addi x30, x0, 0x99" ; "sb x30, 0x500(x0)" |])
  let cpu = CPU (gen.Code)
  cpu.StartUp ()

  Assert.That (cpu.Bus.Dram.Value[0x500], Is.EqualTo 0x99)

[<Test>]
let Lb () =
  use gen =
    new Gen (
      "lb",
      [| "addi x30, x0, 0x99"
         "sb x30, 0x500(x0)"
         "lb x29, 0x500(x0)" |]
    )

  let cpu = CPU (gen.Code)
  cpu.StartUp ()

  Assert.That (cpu.Registers[29], Is.EqualTo 0x99)
