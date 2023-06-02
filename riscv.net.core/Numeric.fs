module riscv.net.core.Numeric

module UInt64 =

    let inline wrapping_add (a : uint64, b : uint64) : uint64 =
        let sum = a + b
        if sum < a || sum < b then a &&& b else sum

    let inline wrapping_sub (a : uint64, b : uint64) =
        let result = a - b

        if result > a then
            result + System.UInt64.MaxValue + 1UL
        else
            result

    let inline wrapping_shr (a : uint64, b : int) : uint64 =
        let mask = 64 - b
        (a >>> b) ||| ((a <<< mask) >>> mask)

    let inline wrapping_shl (a : uint64, b : int) = a <<< int (b &&& 63)


module UInt32 =
    let inline wrapping_shr (a : uint32, b : int) : uint32 =
        let mask = 32 - b
        (a >>> b) ||| ((a <<< mask) >>> mask)

    let inline wrapping_shl (a : uint32, b : int) = a <<< int (b &&& 31)

module Int32 =
    let inline wrapping_shr (a : int32, b : int) : int32 =
        let mask = 32 - b
        (a >>> b) ||| ((a <<< mask) >>> mask)
