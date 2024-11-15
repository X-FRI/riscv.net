module RISCV.NET.Core.Tests.GenerateTestBinary

open System
open System.Runtime.CompilerServices
open RISCV.NET.Core.Tests.FsExecute

type Gen (source : string[], [<CallerMemberName>] ?name : string) =
  let name =
    name |> Option.map (fun name -> name.Trim [| '(' ; ')' |]) |> Option.get

  let compile =
    $"clang -Wl,-Ttext=0x0 -nostdlib --target=riscv64 -march=rv64g -mno-relax -o {name} {name}.s"

  let link = $"llvm-objcopy -O binary {name} {name}.bin"

  do
    IO.File.WriteAllLines ($"{name}.s", source)

    Exec compile
    <|> (Failure, (fun result -> eprintfn $"{result.StandardError}"))
    <|> (Success, (fun result -> printfn $"{result.StandardOutput}"))
    |> ignore

    Exec link
    <|> (Failure, (fun result -> eprintfn $"{result.StandardError}"))
    <|> (Success, (fun result -> printfn $"{result.StandardOutput}"))
    |> ignore

  interface IDisposable with
    member this.Dispose () =
      IO.File.Delete $"{name}"
      IO.File.Delete $"{name}.s"
      IO.File.Delete $"{name}.bin"

  member _.Code = IO.File.ReadAllBytes $"{name}.bin"
