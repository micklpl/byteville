namespace Byteville.Tests
open Xunit
open Byteville.CrawlerLogic
open FSharp.Data.UnitSystems.SI.UnitSymbols
open Byteville.TextMining
open Byteville.Core.Models
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
        Assert.True(data.CreationDate = new System.DateTime(2016, 03, 06))

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
        Assert.True(data.CreationDate = new System.DateTime(2015, 10, 19))

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
        Assert.True(data.CreationDate = new System.DateTime(2016, 3, 30))
    
    [<Fact>]
    member x.Parsing_Without_Errors_Test() = 
        let loader = new Byteville.Core.DataLoader()
        loader.CreateAdvertsIndex() |> ignore

        let dir = "C:/mydir/Projekty/ByteVIlle/src/DataStorage/adverts/"
        let adverts = new System.IO.DirectoryInfo(dir) |> fun di -> di.EnumerateFiles()
                        |> Seq.map(fun file -> Byteville.TextMining.loadFileFromDisk(file.FullName))
                        |> Seq.map(fun stream -> Byteville.TextMining.classifyAdvert(stream))
                        |> Async.Parallel |> Async.RunSynchronously
                        |> Seq.filter(fun ad -> ad.IsSome)
                        |> Seq.map(fun ad -> ad.Value)
                        
                        

        let detected = adverts |> Seq.filter(fun ad -> ad.Street.IsSome) |> Seq.length
        let districtsOnly = adverts |> Seq.filter(fun ad -> ad.Street.IsNone && ad.District.IsSome) |> Seq.length
        let geocoded = adverts |> Seq.filter(fun ad -> ad.Location.IsSome) |> Seq.length

        let withoutDistrict = adverts |> Seq.filter(fun ad -> ad.District.IsNone) |> Seq.length

        let districtFromBody = adverts |> Seq.filter(fun d -> d.Street.IsNone)
                                       |> Seq.map(fun ad -> ad.Description)
                                       |> Seq.toArray
                                       |> String.concat "\n"   
                                               
        
        loader.SendAdverts(adverts)

        Assert.True(true)

    [<Fact>]
    member x.Streets_Parser_From_Title_Helper() = 
        let file = "C:/mydir/Projekty/ByteVIlle/src/DataStorage/titles.txt"
        let tryParseAsync(title) =
            async{
                return(Byteville.TextMining.tryParseStreetFromTitle(title), title)
            }

        let titles = System.IO.File.ReadAllLines(file)
                        |> Seq.map(fun title -> tryParseAsync(title))
                        |> Async.Parallel |> Async.RunSynchronously
          
        let found = titles 
                        |> Seq.filter(fun ad -> fst(ad).IsSome)
                        |> Seq.map(fun pair -> ((fst(pair).Value.Name)+ ", " + snd(pair)))
                        |> Seq.toArray
                        |> String.concat "\n"               
                        
        let nones = titles |> Seq.filter(fun ad -> fst(ad).IsNone)
                            |> Seq.map(fun pair -> snd(pair))
                            |> String.concat "\n"  

        let total = Byteville.TextMining.searchController.Count()
       
        Assert.NotEmpty(found)

    [<Theory>]
    [<InlineData("Mieszkanie 68m2, Prądnik Biały, ul. Natansona", "ULICA WŁADYSŁAWA NATANSONA")>]
    [<InlineData("2 pokoje, 49 m2, os. Sportowe, Nowa Huta", "OSIEDLE SPORTOWE")>]  
    [<InlineData("Kraków, Nowa Huta, Osiedle Urocze, os.Urocze", "OSIEDLE UROCZE")>]
    [<InlineData("Mieszkanie bezpośrednio Kraków Ruczaj Ulica Pszczelna + gratis", "ULICA PSZCZELNA")>]
    [<InlineData("Mieszkanie jednopokojowe - Bochenka", "ULICA ADAMA BOCHENKA")>]
    [<InlineData("Kraków, Podgórze Duchackie, Wola Duchacka, Sanocka", "ULICA SANOCKA")>]
    [<InlineData("Mieszkanie 53m + taras 17m ul.Radzikowskiego kraków", "ULICA WALEREGO ELIASZA RADZIKOWSKIEGO")>]
    [<InlineData("Kraków, Krowodrza, kmieca, Krowodrza, Łobzów", "ULICA KMIECA")>]
    [<InlineData("Piękne mieszkanie 2pok 37m2 ul. Pużaka/Azory", "ULICA KAZIMIERZA PUŻAKA")>]
    [<InlineData("Mieszkanie 98m2/5700zł/m al. Słowackiego!", "ALEJA JULIUSZA SŁOWACKIEGO")>]
    member x.StreetsParser_Test(title:string, expStreetName:string) =
        
        let output =  Byteville.TextMining.tryParseStreetFromTitle(title)

        test <@ output.Value.Name = expStreetName @>

    [<Theory>]
    [<InlineData("Mieszkanie 2 pokoje z jasna kuchnia Gratis wyposażenie")>]
    [<InlineData("Sprzedam mieszkanie w centrum Krakowa")>]
    [<InlineData("Mieszkanie Kraków Wzgórza Krzesławickie 79m2 (nr: 28483)")>]
    [<InlineData("Wszystkie nr bez, Mieszkanie Śródmieście Grzegórzki - Bez Prowizji")>]
    [<InlineData("Mieszkanie, kawalerka Kraków Bronowice sprzedam właściciel")>]
    member x.StreetsParser_Should_Not_Match(title:string) =
        
        let output = Byteville.TextMining.tryParseStreetFromTitle(title)

        test <@ output.IsNone @>

    [<Theory>]
    [<InlineData("usytuowane jest w bloku przy ul.Litewskiej", "ULICA LITEWSKA")>]
    member x.StreetsParser_From_Description(text:string, expectedStreet:string) =
        
        let output = Byteville.TextMining.tryParseStreetFromDescription(text)

        test <@ output.Value.Name = expectedStreet @>

    [<Fact>]
    member x.Transform_Pages_To_Csv() =
        let dir = "C:/mydir/Projekty/ByteVIlle/src/DataStorage/adverts/"
        let adverts = new System.IO.DirectoryInfo(dir) |> fun di -> di.EnumerateFiles()
                        |> Seq.map(fun file -> Byteville.TextMining.loadFileFromDisk(file.FullName))
                        |> Seq.map(fun stream -> Byteville.TextMining.classifyAdvert(stream))
                        |> Async.Parallel |> Async.RunSynchronously
                        |> Seq.filter(fun ad -> ad.IsSome && ad.Value.District.IsSome)
                        |> Seq.map(fun ad -> ad.Value)

        let header = "area;nr_of_rooms;tier;parking;year_of_construction;district_id;price\n"

        let tierMedian = adverts |> Seq.choose(fun a -> a.Tier)
                                 |> Seq.sort|> List.ofSeq 
                                 |> fun lst -> lst.[lst.Length /2]
                                 |> System.Int32.Parse          

        let nrOfRommsMedian = adverts |> Seq.choose(fun a -> a.NumberOfRooms)
                                      |> Seq.sort|> List.ofSeq 
                                      |> fun lst -> lst.[lst.Length /2] 

        let yrOfConstMedian = adverts |> Seq.choose(fun a -> a.YearOfConstruction)
                                      |> Seq.sort|> List.ofSeq 
                                      |> fun lst -> lst.[lst.Length /2] 


        let ppMAvg = adverts |> Seq.groupBy(fun a -> a.District.Value)
                             |> Seq.map(fun t -> (fst(t), snd(t) 
                                                    |> Seq.map(fun a -> a.PricePerMeter) 
                                                    |> Seq.average))
                             |> Seq.sortBy(fun t -> snd(t))
                             |> Seq.map(fun t -> fst(t))
                             |> List.ofSeq
                            
        let evaluateTier(a:Advert) = 
            try
                match a.Tier with
                    | None ->  tierMedian
                    | Some(x) -> match x with
                                   | "parter" -> 0
                                   | _ -> System.Int32.Parse(x)
            with
                | :? System.FormatException -> 0
        
        let adCsv = adverts 
                    |> Seq.map(fun a -> {
                                        a=a.Area;
                                        n= if a.NumberOfRooms.IsSome then a.NumberOfRooms.Value else nrOfRommsMedian;
                                        t= evaluateTier a;
                                        p = if a.Parking.IsSome  then 1 else 0;
                                        y = if a.YearOfConstruction.IsSome then a.YearOfConstruction.Value else yrOfConstMedian
                                        l = ppMAvg |> List.findIndex(fun e -> e.Equals(a.District.Value));
                                        tp = a.TotalPrice
                                    })

                    |> List.ofSeq
                                                
        let path = @"C:\mydir\Projekty\ByteVIlle\src\DataStorage\adverts.csv"
        
        use wr = new System.IO.StreamWriter(path)
        wr.Write header
        adCsv |> List.map(string) |> String.concat("") |> wr.Write
        wr.Close()                                        

        Assert.True(true)
        



        