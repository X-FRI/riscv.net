module RiscV.Net.Test.Program

open NUnit.Framework
open RiscV.Net
open System.IO

[<TestFixture>]
module ASMTests =
    [<Test>]
    let ```test addi`` () =
        let code = "addi x31, x0, 42"
        Utils.riscv_test code "test_addi" 1 (fun cpu -> cpu.regs[31] = 42UL)

module Program =

    [<EntryPoint>]
    let main _ = 0
