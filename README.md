<div align="center">

<img src="./.github/logo.png" height="150px">

# RISC-V.NET

*RISC-V Emualtor running in the Browser, Native or .NET CLR*

__WIP__

</div>

## Introduction

- <a href="https://riscv.org/"> <img src="./.github/riscv-color.svg" height="20px"> RISC-V is an open standard Instruction Set Architecture (ISA) enabling a new era of processor innovation through open collaboration </a>

- <a href="https://dotnet.microsoft.com/en-us/languages/fsharp"> <img src="./.github/fsharp.svg" height="35px"> F# is an open-source language that makes it easy to write succinct, robust, and performant code. </a>

Through F#, you can run a RISC-V Emulator on .NET CLR, or make it run in the native environment through .NET Native AOT.

To run in the browser, there are two solutions:

1. Compile F# to JavaScript via [Fable](fable.io)
2. Make F# run in the WASM environment through [Bolero](https://fsbolero.io/)

## Architecture

```
├  Code running in the browser via JavaScript
├── riscv.net.browser.js
├────────────────────────────────────
├  Code running in the browser via WASM
├── riscv.net.browser.wasm
├────────────────────────────────────
├  The platform independent code
├── riscv.net.core
├────────────────────────────────────
├  The Native and .NET CLR code
└── riscv.net.native
```

## Usage
> project is under development

You can execute the `make test` command under riscv.net.native for testing:

[./riscv.net.native/Makefile](./riscv.net.native/Makefile)
```makefile
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
```

## License
The MIT License (MIT)

Copyright (c) 2022 Muqiu Han

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.