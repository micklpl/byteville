namespace Byteville.Core
open System
open System.IO
open System.Text.RegularExpressions
open System.Web
open Nest
open System.Net
open Newtonsoft.Json
open System.Collections.Specialized
open Byteville.Core.Models
open Newtonsoft.Json.FSharp

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

        {
            Name = name.TrimEnd();  
            AllocationCode = matchResult.Groups.[3].Value; 
            District = matchResult.Groups.[4].Value.TrimEnd()
        }

    
    member x.MatchPattern with get() = matchPattern

    member x.LoadAndFilterStreetsFile filePath = 
        let path = match filePath with
                    | "" -> System.Web.Hosting.HostingEnvironment.MapPath("~/Items/streets.txt")
                    | x -> x
        let matchingLines = 
            File.ReadAllLines(path)
            |> Array.filter(fun line -> not(Regex.IsMatch(line, ignoredUnits)))            
        matchingLines

    member private x.CreateIndex(name, json) =
        let client = new WebClient()
        client.UploadString("http://localhost:9200/" + name, "PUT", json)

    member x.CreateStreetsIndex() =        
        let mapping = """{"mappings":{"administrationunit":{"properties":{"name":{"type":"string","store":"yes","index":"analyzed"},"allocationCode":{"type":"string","store":"yes","index":"not_analyzed"},"district":{"type":"string","store":"yes","index":"not_analyzed"}}}}}"""
        x.CreateIndex("streets", mapping)

    member x.CreateAdvertsIndex() =
        let mapping = """{"mappings":{"advert":{"properties":{"Street":{"type":"string","store":"yes","index":"not_analyzed"}, "District":{"type":"string","store":"yes","index":"not_analyzed"}, "Title":{"type":"string","store":"yes","index":"analyzed"}, "Description":{"type":"string","store":"yes","index":"analyzed"},"Md5":{"type":"string","store":"yes","index":"analyzed"}, "Url":{"type":"string","store":"yes","index":"analyzed"}, "TotalPrice": {"type": "double"}, "PricePerMeter": {"type": "double"}, "Area": {"type": "double"}, "CreationDate": {"type": "date"}}}}}"""
        x.CreateIndex("adverts", mapping)

    member x.IndexStreets(path: String) =        
        let node = new Uri("http://localhost:9200")
        let settings = new ConnectionSettings(node)        
        let client = new ElasticClient(settings.DefaultIndex("streets"))
        x.CreateStreetsIndex() |> ignore

        x.LoadAndFilterStreetsFile(path) 
            |> Array.map(fun line -> x.ParseAdministrationUnit(line)) 
            |> client.IndexMany 
            |> ignore

    member x.IndexExists(name: String) =
        let node = new Uri("http://localhost:9200")
        let settings = new ConnectionSettings(node)
        let client = new ElasticClient(settings.DefaultIndex(name))
        
        client.Count<AdministrationUnit>().Count > 0L

    member x.AdvertToJson(advert:Advert) = 
        let json = JsonConvert.SerializeObject(advert, [| OptionConverter() :> JsonConverter |])
                    .Replace("@","")
        System.Text.Encoding.UTF8.GetBytes(json)

    member x.SendAdverts(adverts:seq<Advert>) =
        let client = new WebClient()
        client.Headers.Add("Content-type: application/json")

        adverts |> Seq.map(fun advert -> x.AdvertToJson(advert)) |>
            Seq.iter(fun json -> client.UploadData("http://localhost:9200/adverts/advert/", 
                                    "POST", json) |> ignore)

