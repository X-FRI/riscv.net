module riscv.net.core.Param

let DRAM_BASE : uint64 = 0x80000000UL

let DRAM_SIZE : uint64 = 1024UL * 1024UL * 128UL

let DRAM_END : uint64 = DRAM_SIZE + DRAM_BASE - 1UL
