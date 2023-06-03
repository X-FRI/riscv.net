module riscv.net.core.test.Numeric

open NUnit.Framework
open riscv.net.core.Numeric

[<TestFixture>]
module TestUInt64 =
    [<Test>]
    let ``WrappingShr should shift bits to the right and wrap around when necessary`` () =
        let x = 0b1000UL
        Assert.AreEqual(UInt64.WrappingShr(x, 1u), 0b0100UL)
        Assert.AreEqual(UInt64.WrappingShr(x, 2u), 0b0010UL)
        Assert.AreEqual(UInt64.WrappingShr(x, 3u), 0b0001UL)
        Assert.AreEqual(UInt64.WrappingShr(x, 4u), 0b1000UL)
        Assert.AreEqual(UInt64.WrappingShr(x, 5u), 0b0100UL)

    [<Test>]
    let ``WrappingShl should shift bits to the left and wrap around when necessary`` () =
        let x = 0b1000UL
        Assert.AreEqual(UInt64.WrappingShl(x, 1u), 0b0001UL)
        Assert.AreEqual(UInt64.WrappingShl(x, 2u), 0b0010UL)
        Assert.AreEqual(UInt64.WrappingShl(x, 3u), 0b0100UL)
        Assert.AreEqual(UInt64.WrappingShl(x, 4u), 0b1000UL)
        Assert.AreEqual(UInt64.WrappingShl(x, 5u), 0b0001UL)

    [<Test>]
    let ``WrappingSub should subtract two values and wrap around when necessary`` () =
        let x = 0b1000UL
        let y = 0b0010UL
        Assert.AreEqual(UInt64.WrappingSub(x, y), 0b0110UL)
        Assert.AreEqual(UInt64.WrappingSub(y, x), 0b1010UL)

    [<Test>]
    let ``WrappingAdd should add two values and wrap around when necessary`` () =
        let x = 0b1000UL
        let y = 0b0010UL
        Assert.AreEqual(UInt64.WrappingAdd(x, y), 0b1010UL)
        Assert.AreEqual(UInt64.WrappingAdd(y, x), 0b1010UL)
