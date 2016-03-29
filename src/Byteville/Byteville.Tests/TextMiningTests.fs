namespace Byteville.Tests
open Xunit
open Byteville.CrawlerLogic
open FSharp.Data.UnitSystems.SI.UnitSymbols
open Byteville.TextMining

type ClassifierTests() = 
    
    [<Fact>]
    member x.OlxAdvertsParserTest() =
        //Arrange
        let filePath = "C:/mydir/Projekty/ByteVIlle/src/DataStorage/adverts/dfeb3cbf0e34c2f5c4583dad96a5c991.html"


        //Act
        let stream = Byteville.TextMining.loadFileFromDisk(filePath)
        let data = Async.RunSynchronously(Byteville.TextMining.classifyAdvert(stream))

        //Assert      
        Assert.True(data.Area = 38M<m^2>)
        Assert.True(data.PricePerMeter = 5526.32M<PLN/m^2>)
        Assert.True(data.NumberOfRooms = 3)
        Assert.True(data.TotalPrice = 210000M<PLN>)

    [<Fact>]
    member x.MorizonAdvertsParserTest() =
        //Arrange
        let filePath = "C:/mydir/Projekty/ByteVIlle/src/DataStorage/adverts/907e73a583141405088d6f57c526f871.html"

        //Act
        let stream = Byteville.TextMining.loadFileFromDisk(filePath)
        let data = Async.RunSynchronously(Byteville.TextMining.classifyAdvert(stream))

        //Assert      
        Assert.True(data.Area = 29.88M<m^2>)
        Assert.True(data.PricePerMeter = 8133M<PLN/m^2>)
        Assert.True(data.NumberOfRooms = 1)
        Assert.True(data.TotalPrice = 243000M<PLN>)

    [<Fact>]
    member x.GumtreeAdvertsParserTest() =
        //Arrange
        let filePath = "C:/mydir/Projekty/ByteVIlle/src/DataStorage/adverts/6b0cbc6a9cbe4630b394033ce9dc2f2c.html"

        //Act
        let stream = Byteville.TextMining.loadFileFromDisk(filePath)
        let data = Async.RunSynchronously(Byteville.TextMining.classifyAdvert(stream))

        //Assert      
        Assert.True(data.Area = 111M<m^2>)
        Assert.True(data.NumberOfRooms = 5)
        Assert.True(data.TotalPrice = 720000M<PLN>)


        