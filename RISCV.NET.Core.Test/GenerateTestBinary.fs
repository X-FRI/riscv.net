module RISCV.NET.Core.Tests.GenerateTestBinary

open System
open RISCV.NET.Core.Tests.FsExecute

type Gen(name: string, source: string[]) =
    let compile =
        $"clang -Wl,-Ttext=0x0 -nostdlib --target=riscv64 -march=rv64g -mno-relax -o {name} {name}.s"

    let link = $"llvm-objcopy -O binary {name} {name}.bin"

    do
        IO.File.WriteAllLines($"{name}.s", source)

        Exec compile
        <|> (Failure, (fun result -> eprintfn $"{result.StandardError}"))
        <|> (Success, (fun result -> printfn $"{result.StandardOutput}"))
        |> ignore

        Exec link
        <|> (Failure, (fun result -> eprintfn $"{result.StandardError}"))
        <|> (Success, (fun result -> printfn $"{result.StandardOutput}"))
        |> ignore

    member _.Code = IO.File.ReadAllBytes $"{name}.bin"
