namespace Byteville.Core
open System
open System.IO
open System.Text.RegularExpressions
open System.Web

[<CLIMutable>]
type AdministrationUnit = { Name : String;  AllocationCode: String; District: String }


type DataLoader() =
    let ignoredUnits = "^(PARK|MOST|KOPIEC|RONDO|\
                              PLANTY|BULWAR|ESTAKADA|OGRÓD|WĘZEŁ|ZAMEK|\
                              ŹRÓDŁO|SKWER|ZJAZD|Strona|Rodzaj)(.*)$"

    let matchPattern = "(ULICA|ALEJA|OSIEDLE|PLAC)\s(.*)([A-ZŚ]{2})\s(.*)"
    let flawedName = "(.*)(Nr.*)"
    let nameWithBrackets = "(.*)(\(.*)";

    member x.ParseAdministrationUnit (str: String) =
        let matchResult = Regex.Match(str, matchPattern)
        let mutable name = matchResult.Groups.[1].Value + " " + matchResult.Groups.[2].Value
        
        let flawedNameMatch = Regex.Match(name, flawedName)
        if flawedNameMatch.Groups.Count = 3 then
            name <- flawedNameMatch.Groups.[1].Value

        let nameWithBracketsMatch = Regex.Match(name, nameWithBrackets)
        if nameWithBracketsMatch.Groups.Count = 3 then
            name <- nameWithBracketsMatch.Groups.[1].Value

        {Name = name.TrimEnd();  AllocationCode = matchResult.Groups.[3].Value; District = matchResult.Groups.[4].Value}

    
    member x.MatchPattern with get() = matchPattern

    member x.LoadAndFilterStreetsFile filePath = 
        let path = match filePath with
                    | "" -> System.Web.Hosting.HostingEnvironment.MapPath("~/Items/streets.txt")
                    | x -> x
        let matchingLines = 
            File.ReadAllLines(path)
            |> Array.filter(fun line -> not(Regex.IsMatch(line, ignoredUnits)))            
        matchingLines
