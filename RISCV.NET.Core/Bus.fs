module RISCV.NET.Core.Bus

open System
open RISCV.NET.Core.Dram
open RISCV.NET.Core.Logger

type Bus (code : inref<uint8[]>) =
  let dram = Dram &code

  do Log.Info "BUS SUCCESSFULLY INSTALLED"

  member public this.Dram = &dram

  member public this.Load (address : UInt64) (size : UInt64) : UInt64 =
    if address >= Dram.BASE && address <= Dram.END then
      dram.Load address size
    else
      failwith $"Load access fault 0x%07X{address}"

  member public this.Store
    (address : UInt64)
    (size : UInt64)
    (value : UInt64)
    : Unit
    =
    if address >= Dram.BASE && address <= Dram.END then
      dram.Store address size value
    else
      failwith $"Store AMO access fault 0x%07X{address}"
