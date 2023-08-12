module RiscV.Net.Error

type t =
    | InstructionAddrMisaligned of uint64
    | InstructionAccessFault of uint64
    | IllegalInstruction of uint64
    | Breakpoint of uint64
    | LoadAccessMisaligned of uint64
    | LoadAccessFault of uint64
    | StoreAMOAddrMisaligned of uint64
    | StoreAMOAccessFault of uint64
    | EnvironmentCallFromUMode of uint64
    | EnvironmentCallFromSMode of uint64
    | EnvironmentCallFromMMode of uint64
    | InstructionPageFault of uint64
    | LoadPageFault of uint64
    | StoreAMOPageFault of uint64

let to_string error =
    match error with
    | InstructionAddrMisaligned(addr) -> $"Instruction address misaligned %X{addr}"
    | InstructionAccessFault(addr) -> $"Instruction access fault %X{addr}"
    | IllegalInstruction(inst) -> $"Illegal instruction %X{inst}"
    | Breakpoint(pc) -> $"Breakpoint %X{pc}"
    | LoadAccessMisaligned(addr) -> $"Load access %X{addr}"
    | LoadAccessFault(addr) -> $"Load access fault %X{addr}"
    | StoreAMOAddrMisaligned(addr) -> $"Store or AMO address misaliged %X{addr}"
    | StoreAMOAccessFault(addr) -> $"Store or AMO access fault %X{addr}"
    | EnvironmentCallFromUMode(pc) -> $"Environment call from U-mode %X{pc}"
    | EnvironmentCallFromSMode(pc) -> $"Environment call from S-mode %X{pc}"
    | EnvironmentCallFromMMode(pc) -> $"Environment call from M-mode %X{pc}"
    | InstructionPageFault(addr) -> $"Instruction page fault %X{addr}"
    | LoadPageFault(addr) -> $"Load page fault %X{addr}"
    | StoreAMOPageFault(addr) -> $"Store or AMO page fault %X{addr}"

let value error =
    match error with
    | InstructionAddrMisaligned(addr) -> addr
    | InstructionAccessFault(addr) -> addr
    | IllegalInstruction(inst) -> inst
    | Breakpoint(pc) -> pc
    | LoadAccessMisaligned(addr) -> addr
    | LoadAccessFault(addr) -> addr
    | StoreAMOAddrMisaligned(addr) -> addr
    | StoreAMOAccessFault(addr) -> addr
    | EnvironmentCallFromUMode(pc) -> pc
    | EnvironmentCallFromSMode(pc) -> pc
    | EnvironmentCallFromMMode(pc) -> pc
    | InstructionPageFault(addr) -> addr
    | LoadPageFault(addr) -> addr
    | StoreAMOPageFault(addr) -> addr

let code error =
    match error with
    | InstructionAddrMisaligned(_) -> 0
    | InstructionAccessFault(_) -> 1
    | IllegalInstruction(_) -> 2
    | Breakpoint(_) -> 3
    | LoadAccessMisaligned(_) -> 4
    | LoadAccessFault(_) -> 5
    | StoreAMOAddrMisaligned(_) -> 6
    | StoreAMOAccessFault(_) -> 7
    | EnvironmentCallFromUMode(_) -> 8
    | EnvironmentCallFromSMode(_) -> 9
    | EnvironmentCallFromMMode(_) -> 11
    | InstructionPageFault(_) -> 12
    | LoadPageFault(_) -> 13
    | StoreAMOPageFault(_) -> 15

let is_fatal error =
    match error with
    | InstructionAddrMisaligned(_)
    | InstructionAccessFault(_)
    | LoadAccessFault(_)
    | StoreAMOAddrMisaligned(_)
    | StoreAMOAccessFault(_)
    | IllegalInstruction(_) -> true
    | _ -> false