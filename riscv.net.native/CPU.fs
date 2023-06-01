module riscv.net.native.CPU 

type CPU(code: array<uint8>) =
    inherit riscv.net.core.CPU.CPU(code)
    
    /// View the state of the registers to verify that the CPU executed instructions correctly.
    member public this.DumpRegisters() =
        printfn "o- REGISTERS"
        
        let __regs = this.REGS
        let mutable i = 0

        __regs[0] <- 0UL

        while i <> 32 do
            printfn
                $"| x{i}({CPU.RVABI[i]}) \t= 0x%02X{__regs[i]} \t|\t x{i + 1}({CPU.RVABI[i + 1]}) \t= 0x%02X{__regs[i + 1]} \t|\t x{i + 2}({CPU.RVABI[i + 2]}) \t= 0x%X{__regs[i + 2]} \t|\t x{i + 3}({CPU.RVABI[i + 3]}) \t= 0x%02X{__regs[i + 3]} \t|"

            i <- i + 4
