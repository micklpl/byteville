module Byteville.TextMining
open System.IO
open FSharp.Data
open System
open FSharp.Data.UnitSystems.SI.UnitSymbols
open System.Text.RegularExpressions
open Byteville.Core.Models
open Byteville.Crawler.Helpers
open Byteville.Crawler.Parsers.Olx
open Byteville.Crawler.Parsers.Morizon
open Byteville.Crawler.Parsers.Gumtree

let loadFileFromDisk path = 
    async{
        use fileStream = File.OpenRead(path)
        let buff = Array.zeroCreate(int(fileStream.Length))
        let! bytes = fileStream.AsyncRead(buff)
        return new MemoryStream(buff) :> Stream
    }

exception NoOgUrlException of string

let searchController = new Byteville.Core.Controllers.SearchController()

let neighboursPattern = "^(ul|ulica|al|aleja|os|osiedle)$"

let findNeighbourhoodWords(tokens:string[]) = 
    Array.FindIndex(tokens, fun token -> Regex.IsMatch(token, neighboursPattern))

let tryGetItem(tokens:string[], index:int) =
    if index >= tokens.Length then 
        None 
    else 
        let mutable item = Some(tokens |> Array.item(index))
        if (index + 1) < tokens.Length then
            let nextToken = tokens |> Array.item(index + 1)
            if Regex.IsMatch(nextToken, "[0-9]{1,3}.*") then
                item <- None
        if item.IsSome && item.Value.Length > 2 then item else None 

let tryParseStreetByNeighbours tokens =     
    match findNeighbourhoodWords(tokens) with
        | -1 -> None
        | index -> tryGetItem(tokens, index + 1)

// http://www.wordcounter.com/
let popularWords = ["osiedle"; "ulica"; "aleja"; "mieszkanie"; "pokoje"; "sprzedam"; "centrum"; 
                    "huta"; "bronowice"; "dębniki"; "pokojowe"; "podgórze"; "prądnik"; "miasto"; "krowodrza";
                    "ruczaj"; "mistrzejowice"; "czerwony"; "biały"; "prokocim"; "bieżanów"; "krakowa"; "jasna";
                    "płaszów"; "słoneczne"; "wzgórza"; "zwierzyniec"; "fałęcki"; "klucz"; "taras"] 

let removeUnnecessaryWords(tokens: String[]) = 
    tokens |> Seq.filter(fun token -> token.Length > 2)
           |> Seq.filter(fun token -> not(popularWords |> List.contains(token)))
           |> Seq.rev // statystycznie częściej nazwa ulicy występuje na końcu tytułu

let blindSearch tokens = 
    tokens |> removeUnnecessaryWords
           |> Seq.map(fun token -> searchController.PrefixSearch(token))
           |> Seq.filter(fun res -> res.Length = 1)
           |> Seq.map(fun res -> res |>Seq.head)
           |> Seq.tryHead

let removeCharacters(str:string) =
    str.Replace("."," ").Replace(",","").Replace("-", " ").Replace("!","").Replace("/"," ")

let tokenize(str: String) = 
    removeCharacters(str).ToLower().Split[|' '|]
        |> Seq.filter(fun token -> not(String.IsNullOrWhiteSpace(token)))

let tryParseStreetFromTitle(title:String) = 
    let tokens = tokenize(title)
                    |> Seq.filter(fun token -> not(token = "kraków")) |> Seq.toArray

    match tryParseStreetByNeighbours(tokens) with
        | None -> blindSearch(tokens)
        | Some(street) -> match searchController.PrefixSearch(street) with
                                | [|x|] -> Some(x)
                                | _ -> blindSearch(tokens)

let tryParseStreetFromDescription(description:String) =
    let tokens = tokenize(description) |> Seq.toArray
    match tryParseStreetByNeighbours(tokens) with
        | None -> None
        | Some(street) -> match searchController.PrefixSearch(street) with
                                | [|x|] -> Some(x)
                                | _ -> None


let classifyAdvert(asyncStream:Async<Stream>) = 
    async{
        let! stream = asyncStream
        let html = HtmlDocument.Load(stream)
        let ogUrl = html.Descendants("meta")
                    |> Seq.find(fun meta -> meta.HasAttribute("property", "og:url"))
                    |> fun node -> node.Attribute("content").Value()

        let advert = 
            try
                match ogUrl with
                    | StartsWith "http://olx.pl" -> Some(olxAdvertParser(html, ogUrl))
                    | StartsWith "http://www.morizon.pl" -> Some(morizonAdvertParser(html, ogUrl))
                    | StartsWith "http://www.gumtree.pl" -> Some(gumtreeAdvertParser(html, ogUrl))
                    | _ -> raise(NoOgUrlException(ogUrl))
            with
                |  :? IncorrectAdvert -> None
        
        if advert.IsSome && advert.Value.Street.IsNone then
            let street = match tryParseStreetFromTitle(advert.Value.Title) with
                            | Some(x) -> Some(x)
                            | _ -> tryParseStreetFromDescription(advert.Value.Description)          

            if street.IsSome then
                advert.Value.Street <- Some(street.Value.Name)
                advert.Value.District <-  Some(street.Value.District)

        return advert
    }