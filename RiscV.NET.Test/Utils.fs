module RiscV.Net.Test.Utils

open RiscV.Net
open System.Diagnostics
open System.IO
open System.Threading.Tasks
open NUnit.Framework

type ExitStatus =
    | Success
    | Failure

type CommandResult =
    { ExitCode : int
      StandardOutput : string
      StandardError : string }

let ExecuteCommand : string -> seq<string> -> Async<CommandResult> =
    fun executable args ->
        async {
            let startInfo = ProcessStartInfo()
            startInfo.FileName <- executable

            for a in args do
                startInfo.ArgumentList.Add(a)

            startInfo.RedirectStandardOutput <- true
            startInfo.RedirectStandardError <- true
            startInfo.UseShellExecute <- false
            startInfo.CreateNoWindow <- true

            use p = new Process()
            p.StartInfo <- startInfo
            p.Start() |> ignore

            let outTask =
                Task.WhenAll(
                    [| p.StandardOutput.ReadToEndAsync()
                       p.StandardError.ReadToEndAsync() |]
                )

            do! p.WaitForExitAsync() |> Async.AwaitTask
            let! out = outTask |> Async.AwaitTask

            return
                { ExitCode = p.ExitCode
                  StandardOutput = out.[0]
                  StandardError = out.[1] }
        }

let Bind : CommandResult -> (ExitStatus * (CommandResult -> unit)) -> CommandResult =
    fun result (status, processer) ->
        (match status with
         | Success ->
             if result.ExitCode = 0 then
                 processer result
         | Failure ->
             if result.ExitCode <> 0 then
                 processer result)

        result

let (<|>) = Bind

let Exec : string -> Async<CommandResult> =
    fun command -> ExecuteCommand "/usr/bin/env" [ "-S"; "bash"; "-c"; command ]

let ExecAsync : string -> CommandResult =
    fun command -> Exec command |> Async.RunSynchronously


let CC = "clang"
let LINK = "llvm-objcopy"

let generate_riscv_binary src =
    printfn $"Generate riscv binary for {src}..."

    async {

        let! obj =
            Exec
                $"{CC} -Wl,-Ttext=0x0 -nostdlib --target=riscv64 -march=rv64g -mno-relax -o {src}.obj {src}.s"

        obj
        <|> (Failure,
             (fun result ->
                 eprintfn
                     $"Failed to generate riscv object: {src}\n{result.StandardError}"))
        <|> (Success, (fun _ -> ()))
        |> ignore

        let! bin = Exec $"{LINK} -O binary {src}.obj {src}.bin"

        bin
        <|> (Failure,
             (fun result ->
                 eprintfn
                     $"Failed to generate riscv binary: {src}\n{result.StandardError}"))
        <|> (Success, (fun _ -> ()))
        |> ignore
    }
    |> Async.RunSynchronously

exception Break of Result<CPU.t, Error.t>
let break_with_result result = raise (Break result)

let rv_helper (code : string) testname n_clock =
    use file = File.Create $"{testname}.s"
    file.Write(System.Text.Encoding.UTF8.GetBytes(code))
    file.Close()
    generate_riscv_binary (testname)

    let cpu = CPU.init (File.ReadAllBytes($"{testname}.bin") |> Array.map uint8)

    try
        for i = 0 to n_clock do
            match CPU.fetch cpu with
            | Ok inst ->
                match CPU.execute cpu inst with
                | Ok() -> break_with_result (Ok(cpu))
                | Error err -> break_with_result (Error err)
            | Error err -> break_with_result (Error err)

        Ok(cpu)
    with Break result ->
        result

let riscv_test code testname n_cloc assert_fn =
    match rv_helper code testname n_cloc with
    | Ok cpu ->
        if assert_fn cpu then
            Assert.Pass()
        else
            CPU.dump_regs cpu
            Assert.Fail()
    | Error(err) -> Assert.Fail $"Name: {testname}\nError: ${Error.to_string err}"