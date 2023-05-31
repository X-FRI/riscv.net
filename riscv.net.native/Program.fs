module riscv.net.localhost


[<EntryPoint>]
let main (args: string array) : int =
    if args.Length < 1 then
        printfn $"riscv.net.localhost binary"

    let cpu = System.IO.File.ReadAllBytes(args[0]) |> riscv.net.core.CPU.CPU in

    try
        while true do
            match cpu.Fetch() with
            | Error(e) -> raise e
            | Ok(instruction) ->
                match cpu.Execute(instruction) with
                | Error(e) -> raise e
                | Ok(pc) -> cpu.PC <- pc
    with e ->
        printfn $"{e}"
        cpu.DumpRegisters()

    0
