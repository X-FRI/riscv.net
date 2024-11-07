<div align="center">

<img src="./.github/logo.png" height="150px">

# RISC-V.NET

*RISC-V Emualtor running in the Browser, Native or .NET CLR*

![](https://img.shields.io/badge/.NET%20Core%208.0.100~preview.7-8A2BE2)
![](https://github.com/muqiuhan/riscv.net/actions/workflows/build.yml/badge.svg)

![](https://img.shields.io/badge/work%20in%20progress-FFFF00)


</div>

## Introduction

- <a href="https://riscv.org/"> <img src="./.github/riscv-color.svg" height="20px"> RISC-V is an open standard Instruction Set Architecture (ISA) enabling a new era of processor innovation through open collaboration </a>

- <a href="https://dotnet.microsoft.com/en-us/languages/fsharp"> <img src="./.github/fsharp.svg" height="35px"> F# is an open-source language that makes it easy to write succinct, robust, and performant code. </a>

Through F#, you can run a RISC-V Emulator on .NET CLR, or make it run in the native environment through .NET Native AOT.

To run in the browser, there are two solutions:

1. Compile F# to JavaScript via [Fable](fable.io)
2. Make F# run in the WASM environment through [Bolero](https://fsbolero.io/)


### Performance Warning

As you can see, this is not a mature solution, and it is almost impossible to become a mature solution. The performance of the simulator built through the above solution will inevitably have serious problems. The value of this project lies in my personal research and study, or to bring inspiration to others `:)`


## Structure
```
.
├── RiscV.NET.Core
├── RiscV.NET.Core.Native.Test
├── RiscV.NET.Native
```

- __RiscV.NET.Core__: The core of RiscV.NET, using .NET Core standard class library and Fable compatible class library to ensure platform independence.
- __RiscV.NET.Core.Native.Test__: Native RiscV.NET.Core tests, not related to RiscV.NET.Native.
- __RiscV.NET.Native__: The native RiscV.NET.Core interface can be compiled into a local executable file through .NET Native AOT to run, or simply run on .NET CLR.

## Reference
- [RISC-V Assembler Reference](https://mark.theis.site/riscv/asm)
- [RISC-V ISA: A rapid way to learn the RISC-V ISA](https://risc-v.guru/instructions/)
- [The RISC-V Instruction Set Manual Volume I: Unprivileged ISA](https://github.com/riscv/riscv-isa-manual/releases/download/Ratified-IMAFDQC/riscv-spec-20191213.pdf)
- [The RISC-V Instruction Set Manual Volume II: Privileged Architecture](https://github.com/riscv/riscv-isa-manual/releases/download/Priv-v1.12/riscv-privileged-20211203.pdf)
- [rvemu: RISC-V emulator for CLI and Web written in Rust with WebAssembly. It supports xv6 and Linux (ongoing)](https://github.com/d0iasm/rvemu)
- [Writing a RISC-V Emulator in Rust](https://book.rvemu.app/)

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