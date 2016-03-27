namespace Byteville.Tests
open Xunit
open Byteville.CrawlerLogic

    

type CrawlerTests() = 

    [<Fact>]
    member x.Check_Olx_Adverts_Page_Parser() =
        //Arrange
        let url = Seq.head(olxAdverts)
        let expPrefix = "http://olx.pl/oferta/"
        let expSuffix = ".html"

        //Act
        let stream = downloadHtmlAsync(url)
        let links = Async.RunSynchronously(parseOlx(stream))

        //Assert
        let areLinksCorrect = links 
                              |> Seq.forall(fun d -> d.StartsWith(expPrefix) && d.EndsWith(expSuffix))       
        Assert.True(areLinksCorrect)


    [<Fact>]
    member x.Check_Olx_Adverts_Second_Page_Parser() =
        //Arrange
        let url = olxAdverts |> Seq.item(1)
        let expPrefix = "http://olx.pl/oferta/"
        let expSuffix = ".html"

        //Act
        let stream = downloadHtmlAsync(url)
        let links = Async.RunSynchronously(parseOlx(stream))

        //Assert
        let areLinksCorrect = links 
                              |> Seq.forall(fun d -> d.StartsWith(expPrefix) && d.EndsWith(expSuffix))       
        Assert.True(areLinksCorrect)

    [<Fact>]
    member x.Check_Morizon_Adverts_Page_Parser() =
        //Arrange
        let url = Seq.head(morizonAdverts)
        let expPrefix = "http://www.morizon.pl/oferta/"

        //Act
        let stream = downloadHtmlAsync(url)
        let links = Async.RunSynchronously(parseMorizon(stream))

        //Assert
        let areLinksCorrect = links |> Seq.forall(fun d -> d.StartsWith(expPrefix))       
        Assert.True(areLinksCorrect)

    [<Fact>]
    member x.Check_Morizon_Adverts_Second_Page_Parser() =
        //Arrange
        let url = morizonAdverts |> Seq.item(1)
        let expPrefix = "http://www.morizon.pl/oferta/"

        //Act
        let stream = downloadHtmlAsync(url)
        let links = Async.RunSynchronously(parseMorizon(stream))

        //Assert
        let areLinksCorrect = links |> Seq.forall(fun d -> d.StartsWith(expPrefix))       
        Assert.True(areLinksCorrect)

    [<Fact>]
    member x.Check_Gumtree_Adverts_Page_Parser() =
        //Arrange
        let url = Seq.head(gumtreeAdverts)
        let expPrefix = "http://www.gumtree.pl/a-mieszkania-i-domy-sprzedam-i-kupie/krakow/"

        //Act
        let stream = downloadHtmlAsync(url)
        let links = Async.RunSynchronously(parseGumtree(stream))

        //Assert
        let areLinksCorrect = links |> Seq.forall(fun d -> d.StartsWith(expPrefix))       
        Assert.True(areLinksCorrect)

    [<Fact>]
    member x.Check_Gumtree_Adverts_Second_Page_Parser() =
        //Arrange
        let url = gumtreeAdverts |> Seq.item(1)
        let expPrefix = "http://www.gumtree.pl/a-mieszkania-i-domy-sprzedam-i-kupie/krakow/"

        //Act
        let stream = downloadHtmlAsync(url)
        let links = Async.RunSynchronously(parseGumtree(stream))

        //Assert
        let areLinksCorrect = links |> Seq.forall(fun d -> d.StartsWith(expPrefix))       
        Assert.True(areLinksCorrect)