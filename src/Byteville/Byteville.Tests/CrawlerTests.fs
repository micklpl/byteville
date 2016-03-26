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
        let stream = Async.RunSynchronously(downloadHtmlAsync(url))
        let links = parseOlx(stream)

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
        let stream = Async.RunSynchronously(downloadHtmlAsync(url))
        let links = parseOlx(stream)

        let arr = Seq.toArray(links)

        //Assert
        let areLinksCorrect = links 
                              |> Seq.forall(fun d -> d.StartsWith(expPrefix) && d.EndsWith(expSuffix))       
        Assert.True(areLinksCorrect)