module RISCV.NET.Core.Tests.Instructions

open System
open NUnit.Framework
open RISCV.NET.Core.CPU
open RISCV.NET.Core.Tests.GenerateTestBinary
open RISCV.NET.Core.StartUp

[<Test>]
let Addi () =
  use gen = new Gen [| "addi x29, x0, 5" ; "addi x30, x0, 37" |]
  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[29], Is.EqualTo 5)
  Assert.That (cpu.Registers[30], Is.EqualTo 37)

[<Test>]
let Add () =
  use gen =
    new Gen [| "addi x29, x0, 5"
               "addi x30, x0, 37"
               "add x31, x30, x29" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[31], Is.EqualTo 42)

[<Test>]
let Sb () =
  use gen = new Gen [| "addi x30, x0, 0x99" ; "sb x30, 0x500(x0)" |]
  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Bus.Dram.Value[0x500], Is.EqualTo 0x99)

[<Test>]
let Lb () =
  use gen =
    new Gen [| "addi x30, x0, 0x99"
               "sb x30, 0x500(x0)"
               "lb x29, 0x500(x0)" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[29], Is.EqualTo 0x99)

[<Test>]
let Lh () =
  use gen =
    new Gen [| "addi x30, x0, 0x99"
               "sb x30, 0x500(x0)"
               "lh x29, 0x500(x0)" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[29], Is.EqualTo 0x99)

[<Test>]
let Lw () =
  use gen =
    new Gen [| "addi x30, x0, 0x99"
               "sb x30, 0x500(x0)"
               "lw x29, 0x500(x0)" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[29], Is.EqualTo 0x99)

[<Test>]
let Lbu () =
  use gen =
    new Gen [| "addi x30, x0, 0x99"
               "sb x30, 0x500(x0)"
               "lbu x29, 0x500(x0)" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[29], Is.EqualTo 0x99)

[<Test>]
let Lwu () =
  use gen =
    new Gen [| "addi x30, x0, 0x99"
               "sb x30, 0x500(x0)"
               "lwu x29, 0x500(x0)" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[29], Is.EqualTo 0x99)

[<Test>]
let Lhu () =
  use gen =
    new Gen [| "addi x30, x0, 0x99"
               "sb x30, 0x500(x0)"
               "lhu x29, 0x500(x0)" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[29], Is.EqualTo 0x99)

[<Test>]
let Ld () =
  use gen = new Gen [| "ld x28, 0x500(x0)" |]

  let cpu = CPU gen.Code

  let test = BitConverter.GetBytes (9999UL)
  Array.blit test 0 cpu.Bus.Dram.Value 0x500 test.Length

  cpu.StartUp ()

  Assert.That (cpu.Registers[28], Is.EqualTo 9999UL)
