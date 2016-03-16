namespace Byteville.Tests
open Xunit
open System.IO
open System.Text.RegularExpressions
open System

type DataLoadingTests() = 

    [<Fact>]
    member x.Check_Streets_File_For_Proper_Data_Format() =
        let matchPattern = "(ULICA|ALEJA|OSIEDLE|PLAC)\s(.*)[A-ZŚ]{2}\s(.*)"
        let ignoredUnits = "^(PARK|MOST|KOPIEC|RONDO|\
                              PLANTY|BULWAR|ESTAKADA|OGRÓD|WĘZEŁ|ZAMEK|\
                              ŹRÓDŁO|SKWER|ZJAZD|Strona|Rodzaj)(.*)$"
             
        let oddLines = 
            File.ReadAllLines("Items/streets.txt") 
            |> Array.filter(fun line -> not(Regex.IsMatch(line, matchPattern)))
            |> Array.filter(fun line -> not(Regex.IsMatch(line, ignoredUnits)))
            |> fun d -> d.Length

        Assert.Equal(0, oddLines);
