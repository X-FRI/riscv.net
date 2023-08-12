open NUnit.Framework
open RiscV.Net.CPU
open System.IO

[<TestFixture>]
module Tests =
    let TEST_BINARY_PATH = "../../../../TestBinary"

    [<Test>]
    let ``add-addi.bin`` () =
        CPU(File.ReadAllBytes($"{TEST_BINARY_PATH}/add-addi.bin") |> Array.map uint8)
            .Run()

module Program =

    [<EntryPoint>]
    let main _ = 0
