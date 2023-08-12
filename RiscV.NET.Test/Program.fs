open NUnit.Framework
open RiscV.Net
open System.IO

[<TestFixture>]
module Tests =
    let TEST_BINARY_PATH = "../../../../TestBinary"

    let fail cpu err =
        CPU.dump_regs cpu
        Assert.Fail $"{Error.to_string err}"

    [<Test>]
    let ``add-addi.bin`` () =
        let cpu =
            CPU.init (File.ReadAllBytes($"{TEST_BINARY_PATH}/add-addi.bin") |> Array.map uint8)

        match CPU.run cpu with
        | Ok() ->
            CPU.dump_regs cpu
            Assert.Fail()

        | Error err ->
            match err with
            | Error.IllegalInstruction 0UL ->
                if
                    cpu.regs[31] = 0x2aUL
                    && cpu.regs[30] = 0x25UL
                    && cpu.regs[2] = 0x87ffffffUL
                    && cpu.regs[29] = 0x5UL
                then
                    Assert.Pass()
                else
                    fail cpu err
            | _ -> fail cpu err



module Program =

    [<EntryPoint>]
    let main _ = 0
