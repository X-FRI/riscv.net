module riscv.net.localhost


[<EntryPoint>]
let main (args: string array) : int =
    if args.Length < 1 then
        printfn $"riscv.net.localhost binary"

    let cpu = System.IO.File.ReadAllBytes(args[0]) |> riscv.net.core.CPU.CPU in

    try
        while cpu.PC < uint64 (cpu.DRAM.Length) do
            cpu.Execute(cpu.Fetch())
    with :? System.Exception as e ->
        printfn $"{e.Message}"
        cpu.DumpRegisters()

    0
