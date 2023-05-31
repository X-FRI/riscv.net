add-addi.bin : add-addi
	llvm-objcopy -O binary add-addi add-addi.bin

add-addi : add-addi.s
	clang -Wl,-Ttext=0x0 -nostdlib --target=riscv64 -march=rv64g -mno-relax -o add-addi add-addi.s

test:
	make add-addi.bin
	dotnet run -- add-addi.bin

clean:
	rm add-addi add-addi.bin
	dotnet clean