module RiscV.NET.Native.CPU

open RiscV.NET.Core

let dump_regs (cpu : CPU.t) =
    printfn $"o- Registers"
    printfn $"o- pc = %X{cpu.pc}"

    cpu.regs[0] <- 0UL

    for i in 0..4..31 do
        printfn
            $"x{i}({Register.RVABI[i]}) = 0x%0x{cpu.regs[i]}\tx{i + 1}({Register.RVABI[i + 1]}) = 0x%0x{cpu.regs[i + 1]}\tx{i + 2}({Register.RVABI[i + 2]}) = 0x%0x{cpu.regs[i + 2]}\tx{i + 3}({Register.RVABI[i + 3]}) = 0x%0x{cpu.regs[i + 3]}"
