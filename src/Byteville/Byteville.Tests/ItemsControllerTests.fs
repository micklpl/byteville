namespace Byteville.Tests
open Xunit // import namespace


type MyTest() = 
    let add x y = x + y // unit

    [<Fact>]   // test
    let add_5_to_3_should_be_8() =
        Assert.Equal(3, add 5 3)