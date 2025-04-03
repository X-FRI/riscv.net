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

[<Test>]
let Addiw () =
  use gen = new Gen [| "addiw x29, x0, 5" |]
  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[29], Is.EqualTo 5UL)

[<Test>]
let Addw () =
  use gen =
    new Gen [| "addi x29, x0, 5"
               "addi x30, x0, 10"
               "addw x31, x30, x29" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[31], Is.EqualTo 15UL)

[<Test>]
let Subw () =
  use gen =
    new Gen [| "addi x29, x0, 20"
               "addi x30, x0, 7"
               "subw x31, x29, x30" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[31], Is.EqualTo 13UL)

[<Test>]
let Slli () =
  use gen = new Gen [| "addi x29, x0, 3" ; "slli x30, x29, 2" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[30], Is.EqualTo 12UL)

[<Test>]
let Srli () =
  use gen = new Gen [| "addi x29, x0, 16" ; "srli x30, x29, 2" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[30], Is.EqualTo 4UL)

[<Test>]
let Srai () =
  use gen =
    new Gen [| "addi x29, x0, -8" // direct use -8, ensure it is negative
               "srai x30, x29, 1" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  // because of arithmetic right shift, the sign bit should be retained, causing the high bit to be extended
  Assert.That (cpu.Registers[30] &&& 0xFFUL, Is.EqualTo 0xFCUL)

[<Test>]
let Slliw () =
  use gen = new Gen [| "addi x29, x0, 3" ; "slliw x30, x29, 2" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[30], Is.EqualTo 12UL)

[<Test>]
let Srliw () =
  use gen = new Gen [| "addi x29, x0, 16" ; "srliw x30, x29, 2" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[30], Is.EqualTo 4UL)

[<Test>]
let Sraiw () =
  use gen =
    new Gen [| "addi x29, x0, -8" // direct use -8, ensure it is negative
               "sraiw x30, x29, 1" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  // arithmetic right shift, the sign bit should be retained
  Assert.That (cpu.Registers[30] &&& 0xFFUL, Is.EqualTo 0xFCUL)

[<Test>]
let Sllw () =
  use gen =
    new Gen [| "addi x29, x0, 3"
               "addi x28, x0, 2"
               "sllw x30, x29, x28" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[30], Is.EqualTo 12UL)

[<Test>]
let Srlw () =
  use gen =
    new Gen [| "addi x29, x0, 16"
               "addi x28, x0, 2"
               "srlw x30, x29, x28" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  Assert.That (cpu.Registers[30], Is.EqualTo 4UL)

[<Test>]
let Sraw () =
  use gen =
    new Gen [| "addi x29, x0, -8" // direct use -8, ensure it is negative
               "addi x28, x0, 1"
               "sraw x30, x29, x28" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  // arithmetic right shift, the sign bit should be retained
  Assert.That (cpu.Registers[30] &&& 0xFFUL, Is.EqualTo 0xFCUL)

[<Test>]
let Sd () =
  use gen = new Gen [| "addi x30, x0, 0x42" ; "sd x30, 0x520(x0)" |]

  let cpu = CPU gen.Code
  cpu.StartUp ()

  let value = cpu.Bus.Load 0x520UL 64UL
  Assert.That (value &&& 0xFFUL, Is.EqualTo 0x42UL)
