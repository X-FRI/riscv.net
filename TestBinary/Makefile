fib.bin: fib.c
	clang -Wl,-Ttext=0x0 -nostdlib --target=riscv64 -march=rv64g -mno-relax -S fib.c
	clang -Wl,-Ttext=0x0 -nostdlib --target=riscv64 -march=rv64g -mno-relax -o fib.obj fib.s
	llvm-objcopy -O binary fib.obj fib.bin

simple.bin: simple.c
	clang -Wl,-Ttext=0x0 -nostdlib --target=riscv64 -march=rv64g -mno-relax -S simple.c
	clang -Wl,-Ttext=0x0 -nostdlib --target=riscv64 -march=rv64g -mno-relax -o simple.obj simple.s
	llvm-objcopy -O binary simple.obj simple.bin

clean:
	rm *.obj *.bin