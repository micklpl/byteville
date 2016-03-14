namespace Byteville.Tests
open Xunit // import namespace
open Byteville.Core.Controllers


type ItemsControllerTests() = 

    [<Fact>]
    member x.When_Removed_Element_Expect_To_Retrieve_One_Element_Less() =         
        let controller = new ItemsController()
        let count = controller.Get().Length

        controller.Delete(0) |> ignore

        Assert.Equal(count - 1, controller.Get().Length)

    [<Fact>]
    member x.When_Added_Element_Expect_To_Retrieve_One_Element_More() = 
        let controller = new ItemsController()
        let count = controller.Get().Length
        let item = {name="test"; id=3}

        controller.Post(item) |> ignore

        Assert.Equal(count + 1, controller.Get().Length)