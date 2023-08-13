module RiscV.Net.Test.Program

open NUnit.Framework
open RiscV.Net

[<Test>]
let test_addi () =
    let code = "addi x31, x0, 42"
    Utils.riscv_test code "test_addi" 1 (fun cpu -> cpu.regs[31] = 42UL)

[<Test>]
let test_simple () =
    let code =
        "
            addi sp, sp,-16
            sd	 s0, 8(sp)
            addi s0, sp, 16
            li	 a5, 42
            mv	 a0, a5
            ld	 s0, 8(sp)
            addi sp, sp, 16
            jr	 ra
        "

    Utils.riscv_test code "test_simple" 20 (fun cpu -> cpu.regs[10] = 42UL)

[<Test>]
let test_lui () =
    let code = "lui a0, 42"

    Utils.riscv_test code "test_lui" 1 (fun cpu -> cpu.regs[10] = (42UL <<< 12))

[<Test>]
let test_auipc () =
    let code = "auipc a0, 42"

    Utils.riscv_test code "test_auipc" 1 (fun cpu ->
        cpu.regs[10] = (Dram.BASE + (42UL <<< 12)))

[<Test>]
let test_jal () =
    let code = "jal a0, 42"

    Utils.riscv_test code "test_jal" 1 (fun cpu ->
        (cpu.regs[10] = (Dram.BASE + 4UL)) && (cpu.pc = (Dram.BASE + 42UL)))

[<Test>]
let test_jalr () =
    let code =
        "
            addi a1, zero, 42
            jalr a0, -8(a1)
        "

    Utils.riscv_test code "test_jalr" 2 (fun cpu ->
        (cpu.regs[10] = Dram.BASE + 8UL) && (cpu.pc = 34UL))

[<Test>]
let test_beq () =
    let code = "beq  x0, x0, 42"

    Utils.riscv_test code "test_beq" 3 (fun cpu -> cpu.pc = (Dram.BASE + 42UL))

[<Test>]
let test_bne () =
    let code =
        "
            addi x1, x0, 10
            bne  x0, x1, 42
        "

    Utils.riscv_test code "test_bne" 5 (fun cpu -> cpu.pc = (Dram.BASE + 42UL + 4UL))

[<Test>]
let test_blt () =
    let code =
        "
            addi x1, x0, 10
            addi x2, x0, 20
            blt  x1, x2, 42
        "

    Utils.riscv_test code "test_blt" 10 (fun cpu -> cpu.pc = (Dram.BASE + 42UL + 8UL))

[<Test>]
let test_bge () =
    let code =
        "
            addi x1, x0, 10
            addi x2, x0, 20
            bge  x2, x1, 42
        "

    Utils.riscv_test code "test_bge" 10 (fun cpu -> cpu.pc = (Dram.BASE + 42UL + 8UL))

[<Test>]
let test_bltu () =
    let code =
        "
            addi x1, x0, 10
            addi x2, x0, 20
            bltu x1, x2, 42
        "

    Utils.riscv_test code "test_bltu" 10 (fun cpu -> cpu.pc = (Dram.BASE + 42UL + 8UL))

[<Test>]
let test_bgeu () =
    let code =
        "
            addi x1, x0, 10
            addi x2, x0, 20
            bgeu x2, x1, 42
        "

    Utils.riscv_test code "test_bgeu" 10 (fun cpu -> cpu.pc = (Dram.BASE + 42UL + 8UL))

[<Test>]
let test_store_load1 () =
    let code =
        "
            addi s0, zero 256
            addi sp, sp -16
            sd   s0, 8(sp)
            lb   t1, 8(sp)
            lh   t2, 8(sp)
        "

    Utils.riscv_test code "test_store_load1" 10 (fun cpu ->
        (cpu.regs[6] = 0UL) && (cpu.regs[7] = 256UL))

[<Test>]
let test_slt () =
    let code =
        "
            addi t0, zero, 14
            addi t1, zero, 24
            slt  t2, t0, t1
            slti t3, t0, 42
            sltiu t4, t0, 84
        "

    Utils.riscv_test code "test_slt" 7 (fun cpu ->
        (cpu.regs[7] = 1UL) && (cpu.regs[28] = 1UL) && (cpu.regs[29] = 1UL))

[<Test>]
let test_xor () =
    let code =
        "
            addi a0, zero, 0b10
            xori a1, a0, 0b01
            xor a2, a1, a1 
        "

    Utils.riscv_test code "test_xor" 5 (fun cpu ->
        (cpu.regs[11] = 3UL) && (cpu.regs[12] = 0UL))

[<Test>]
let test_or () =
    let code =
        "
            addi a0, zero, 0b10
            ori  a1, a0, 0b01
            or   a2, a0, a0
        "

    Utils.riscv_test code "test_or" 3 (fun cpu ->
        (cpu.regs[11] = 0b11UL) && (cpu.regs[12] = 0b10UL))

[<Test>]
let test_and () =
    let code =
        "
            addi a0, zero, 0b10 
            andi a1, a0, 0b11
            and  a2, a0, a1
        "

    Utils.riscv_test code "test_and" 3 (fun cpu ->
        (cpu.regs[11] = 0b10UL) && (cpu.regs[12] = 0b10UL))

[<Test>]
let test_sll () =
    let code =
        "
            addi a0, zero, 1
            addi a1, zero, 5
            sll  a2, a0, a1
            slli a3, a0, 5
            addi s0, zero, 64
            sll  a4, a0, s0
        "

    Utils.riscv_test code "test_sll" 10 (fun cpu ->
        (cpu.regs[12] = (1UL <<< 5))
        && (cpu.regs[13] = (1UL <<< 5))
        && (cpu.regs[14] = 1UL))

[<Test>]
let test_sra_srl () =
    let code =
        "
            addi a0, zero, -8
            addi a1, zero, 1
            sra  a2, a0, a1
            srai a3, a0, 2
            srli a4, a0, 2
            srl  a5, a0, a1
        "

    Utils.riscv_test code "test_sra_srl" 10 (fun cpu ->
        (cpu.regs[12] = (-4 |> int64 |> uint64))
        && (cpu.regs[13] = (-2 |> int64 |> uint64))
        && (cpu.regs[14] = (-8 |> int64 |> uint64 >>> 2))
        && (cpu.regs[15] = (-8 |> int64 |> uint64 >>> 1)))

[<Test>]
let test_word_op () =
    let code =
        "
            addi a0, zero, 42 
            lui  a1, 0x7f000
            addw a2, a0, a1
        "

    Utils.riscv_test code "test_word_op" 29 (fun cpu -> cpu.regs[12] = 0x7f00002aUL)


module Program =

    [<EntryPoint>]
    let main _ = 0
