namespace Byteville.Tests
open Xunit
open System.IO
open System.Text.RegularExpressions
open System
open Byteville.Core

type DataLoadingTests() = 

    [<Fact>]
    member x.Check_Streets_File_For_Proper_Data_Format() =
        let dataLoader = new DataLoader()

        let oddLines = dataLoader.LoadAndFilterStreetsFile("Items/streets.txt")
                        |> Array.filter(
                            fun line -> not(Regex.IsMatch(line, dataLoader.MatchPattern))) 
                        |> fun d -> d.Length

        Assert.Equal(0, oddLines);

    [<Fact>]
    member x.Simple_Street_Parsing_Test() =
        let strLine = "ULICA GABRIELI ZAPOLSKIEJ KR Bronowice";
        let dataLoader = new DataLoader()


        let result = dataLoader.ParseAdministrationUnit(strLine)

        let areEqual = (result = { Name = "ULICA GABRIELI ZAPOLSKIEJ"; AllocationCode = "KR"; District = "Bronowice" })
        Assert.True(areEqual)

    [<Fact>]
    member x.Street_With_Unnecessary_Data_Parsing_Test() =
        let strLine = "ALEJA 29 LISTOPADA Nr.1-39 b, 2-36, brak nr.38 i 40 SM Stare Miasto";
        let dataLoader = new DataLoader()


        let result = dataLoader.ParseAdministrationUnit(strLine)

        let areEqual = (result = { Name = "ALEJA 29 LISTOPADA"; AllocationCode = "SM"; District = "Stare Miasto" })
        Assert.True(areEqual)

    [<Fact>]
    member x.Street_With_Brackets_Parsing_Test() =
        let strLine = "ALEJA GENERAŁA WŁADYSŁAWA ANDERSA (do ul.Braci Schindlerów) brak zabudowy ŚR Grzegórzki";
        let dataLoader = new DataLoader()

        let result = dataLoader.ParseAdministrationUnit(strLine)

        let areEqual = (result = { Name = "ALEJA GENERAŁA WŁADYSŁAWA ANDERSA"; AllocationCode = "ŚR"; District = "Grzegórzki" })
        Assert.True(areEqual)