namespace Byteville.Tests
open Xunit
open Byteville.CrawlerLogic

type ClassifierTests() = 
    
    [<Fact>]
    member x.Test() =
        //Arrange
        let url = "http://olx.pl/oferta/mieszkanie-dwa-pokoje-51m2-po-remoncie-pawla-wlodkowica-super-cena-CID3-IDeE5sL.html#fee700140e;promoted"
        let filePath = "C:/mydir/Projekty/ByteVIlle/src/DataStorage/adverts/0d05cf22302ca9b1430b1e22cccd3043.html"

        //Act
        let stream = Byteville.TextMining.loadFileFromDisk(filePath)
        let data = Async.RunSynchronously(Byteville.TextMining.classifyAdvert(stream))

        //Assert      
        Assert.True(true)