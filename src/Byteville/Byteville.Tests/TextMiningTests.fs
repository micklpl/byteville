namespace Byteville.Tests
open Xunit
open Byteville.CrawlerLogic
open FSharp.Data.UnitSystems.SI.UnitSymbols
open Byteville.TextMining
open Swensen.Unquote

type ClassifierTests() = 
    
    [<Fact>]
    member x.OlxAdvertsParserTest() =
        //Arrange
        let filePath = "C:/mydir/Projekty/ByteVIlle/src/DataStorage/adverts/dfeb3cbf0e34c2f5c4583dad96a5c991.html"

        //Act
        let stream = Byteville.TextMining.loadFileFromDisk(filePath)
        let data = Async.RunSynchronously(Byteville.TextMining.classifyAdvert(stream)).Value

        //Assert      
        Assert.True(data.Area = 38M<m^2>)
        Assert.True(data.PricePerMeter = 5526.32M<PLN/m^2>)
        Assert.True(data.NumberOfRooms = Some 3)
        Assert.True(data.TotalPrice = 210000M<PLN>)

    [<Fact>]
    member x.MorizonAdvertsParserTest() =
        //Arrange
        let filePath = "C:/mydir/Projekty/ByteVIlle/src/DataStorage/adverts/907e73a583141405088d6f57c526f871.html"

        //Act
        let stream = Byteville.TextMining.loadFileFromDisk(filePath)
        let data = Async.RunSynchronously(Byteville.TextMining.classifyAdvert(stream)).Value

        //Assert      
        Assert.True(data.Area = 29.88M<m^2>)
        Assert.True(data.PricePerMeter = 8133M<PLN/m^2>)
        Assert.True(data.NumberOfRooms = Some 1)
        Assert.True(data.TotalPrice = 243000M<PLN>)

    [<Fact>]
    member x.GumtreeAdvertsParserTest() =
        //Arrange
        let filePath = "C:/mydir/Projekty/ByteVIlle/src/DataStorage/adverts/4ddb9b4def56994e5dd17e2e244bb7c8.html"

        //Act
        let stream = Byteville.TextMining.loadFileFromDisk(filePath)
        let data = Async.RunSynchronously(Byteville.TextMining.classifyAdvert(stream)).Value

        //Assert      
        Assert.True(data.Area = 36M<m^2>)
        Assert.True(data.NumberOfRooms = None)
        Assert.True(data.TotalPrice = 220000M<PLN>)
    
    [<Fact>]
    member x.Parsing_Without_Errors_Test() = 
        let dir = "C:/mydir/Projekty/ByteVIlle/src/DataStorage/adverts/"
        let adverts = new System.IO.DirectoryInfo(dir) |> fun di -> di.EnumerateFiles()
                        |> Seq.map(fun file -> Byteville.TextMining.loadFileFromDisk(file.FullName))
                        |> Seq.map(fun stream -> Byteville.TextMining.classifyAdvert(stream))
                        |> Async.Parallel |> Async.RunSynchronously
                        |> Seq.toArray

        Assert.NotEmpty(adverts)

    [<Fact>]
    member x.StreetsParser_Helper() = 
        let file = "C:/mydir/Projekty/ByteVIlle/src/DataStorage/titles.txt"
        let titles = System.IO.File.ReadAllLines(file)
                        |> Seq.map(fun title -> (Byteville.TextMining.tryParseStreet(title)), title)
          
        let found = titles 
                        |> Seq.filter(fun ad -> fst(ad).IsSome)
                        |> Seq.map(fun pair -> ((fst(pair).Value.Name)+ ", " + snd(pair)))
                        |> Seq.toArray
                        |> String.concat "\n"               
                        
        let nones = titles |> Seq.filter(fun ad -> fst(ad).IsNone)
                            |> Seq.map(fun pair -> snd(pair))
                            |> String.concat "\n"  
       
        Assert.NotEmpty(found)

    [<Theory>]
    [<InlineData("Mieszkanie 68m2, Prądnik Biały, ul. Natansona", "ULICA WŁADYSŁAWA NATANSONA")>]
    [<InlineData("2 pokoje, 49 m2, os. Sportowe, Nowa Huta", "OSIEDLE SPORTOWE")>]  
    [<InlineData("Kraków, Nowa Huta, Osiedle Urocze, os.Urocze", "OSIEDLE UROCZE")>]
    [<InlineData("Mieszkanie bezpośrednio Kraków Ruczaj Ulica Pszczelna + gratis", "ULICA PSZCZELNA")>]
    [<InlineData("Mieszkanie jednopokojowe - Bochenka", "ULICA ADAMA BOCHENKA")>]    
    member x.StreetsParser_Test(title:string, expStreetName:string) =
        
        let output =  Byteville.TextMining.tryParseStreet(title)

        test <@ output.Value.Name = expStreetName @>
        



        