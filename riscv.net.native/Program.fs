module riscv.net.native.Program

open riscv.net.native.CPU

[<EntryPoint>]
let main (args: string array) : int =
    if args.Length < 1 then
        printfn $"riscv.net.localhost binary"

    let cpu = System.IO.File.ReadAllBytes(args[0]) |> CPU in

    try
        while true do
            cpu.PC <- cpu.Execute(cpu.Fetch())
    with e ->
        printfn $"{e}"
        cpu.DumpRegisters()

    0
