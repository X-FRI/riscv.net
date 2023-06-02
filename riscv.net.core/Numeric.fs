module riscv.net.core.Numeric

module UInt64 = 
    let inline wrapping_shr (a : uint64, b : int) : uint64 =
        let mask = 64 - b
        (a >>> b) ||| ((a <<< mask) >>> mask)
        
    let inline wrapping_add (a : uint64, b : uint64) : uint64 =
        let sum = a + b
        if sum < a || sum < b then a &&& b else sum

module UInt32 = 
    let inline wrapping_shr (a : uint32, b : int) : uint32 =
        let mask = 32 - b
        (a >>> b) ||| ((a <<< mask) >>> mask)

module Int32 =
    let inline wrapping_shr (a : int32, b : int) : int32 =
        let mask = 32 - b
        (a >>> b) ||| ((a <<< mask) >>> mask)