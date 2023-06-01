module riscv.net.core.Exception

exception LoadAccessFault of uint64
exception StoreAMOAccessFault of uint64
exception IllegalInstruction of uint64
