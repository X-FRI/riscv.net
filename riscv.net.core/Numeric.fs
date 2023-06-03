module riscv.net.core.Numeric

open System

module UInt64 =
    let inline WrappingAdd (x : uint64, y : uint64) : uint64 =
        let result = x + y

        if result < x || result < y then
            result - UInt64.MaxValue + 1UL
        else
            result


    let inline WrappingSub (x : uint64, y : uint64) : uint64 = (x - y) % (UInt64.MaxValue + 1UL)
    let inline WrappingShr (x : uint64, shift : uint32) : uint64 = x >>> ((shift % 64u) |> int)
    let inline WrappingShl (x : uint64, shift : uint32) : uint64 = x <<< ((shift % 64u) |> int)

module UInt32 =
    let inline WrappingAdd (x : uint32, y : uint32) : uint32 =
        let result = x + y

        if result < x || result < y then
            result - UInt32.MaxValue + 1u
        else
            result

    let inline WrappingSub (x : uint32, y : uint32) : uint32 = (x - y) % (UInt32.MaxValue + 1u)
    let inline WrappingShr (x : uint32, shift : uint32) : uint32 = x >>> (int shift % 32)
    let inline WrappingShl (x : uint32, shift : uint32) : uint32 = x <<< (int shift % 32)

module Int32 =
    let inline WrappingAdd (x : int32, y : int32) : int32 =
        let result = x + y

        if result < x || result < y then
            result - Int32.MaxValue + 1
        else
            result

    let inline WrappingSub (x : int32, y : int32) : int32 = (x - y) % (Int32.MaxValue + 1)
    let inline WrappingShr (x : int32, shift : int32) : int32 = x >>> (int shift % 32)
    let inline WrappingShl (x : int32, shift : int32) : int32 = x <<< (int shift % 32)
