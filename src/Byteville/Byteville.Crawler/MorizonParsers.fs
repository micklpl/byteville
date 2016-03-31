module Byteville.Crawler.Parsers.Morizon
open System.IO
open FSharp.Data
open Byteville.Core.Models
open System
open Byteville.Crawler.Helpers
open FSharp.Data.UnitSystems.SI.UnitSymbols

let morizonAdverts = 
    seq { 
        let urlBase = "http://www.morizon.pl/mieszkania/krakow/"
        yield urlBase

        for page in 2 .. 200 do
            yield urlBase + "?page=" + page.ToString()
    }

let parseMorizon(streamAsync:Async<Stream>) = 
    async{
        let! stream = streamAsync
        let html = HtmlDocument.Load(stream)    
        return html.Descendants["div"]
            |> Seq.filter(fun t-> t.HasAttribute("class", "listingBox mainBox propertyListingBox"))
            |> Seq.head
            |> fun table -> table.Descendants["a"]
            |> Seq.map(fun a -> a.AttributeValue("href"))
            |> Seq.filter(fun link -> link.StartsWith("http://www.morizon.pl/oferta/"))
            |> Seq.distinct
    }

let seekMorizonList(listItems:seq<string*HtmlNode>, desc:String) =
    listItems |> Seq.find(fun pair -> (fst(pair)).Contains(desc)) 
        |> (fun pair -> ((snd(pair)).InnerText()))

let trySeekMorizonList(listItems:seq<string*HtmlNode>, desc:String) = 
    try
        Some(seekMorizonList(listItems, desc))
    with 
        |  :? System.Collections.Generic.KeyNotFoundException -> None    

let polishStringToBoolaen(str) = 
    match str with
            | "Tak" -> Some true 
            | "Nie" -> Some false
            | _     -> None

let trySeekMorizonListForBoolean(listItems:seq<string*HtmlNode>, desc:String) =
    match trySeekMorizonList(listItems, desc) with
        | Some(x) -> polishStringToBoolaen(x)
        | None -> None

let trySeekMorizonListForInt(listItems:seq<string*HtmlNode>, desc:String) =
    match trySeekMorizonList(listItems, desc) with
        | Some(x) -> Some(System.Int32.Parse(x))
        | None -> None

    
let morizonAdvertParser(html:HtmlDocument, link:String) =
    let md5 = md5(link)
    let title = html.Descendants("h1") |> Seq.head |> fun h1-> h1.Descendants("strong") 
                 |> Seq.head |> fun node -> node.InnerText()

    let descriptionDiv = html.Descendants("div") |> Seq.find(fun div -> div.HasAttribute("id", "description"))

    let descriptionSpan = descriptionDiv.Descendants("span") |> Seq.tryHead

    let description = match descriptionSpan with
                        | Some(span) -> span.InnerText()
                        | None -> descriptionDiv.InnerText()

    let summaryPrice = html.Descendants("div") 
                        |> Seq.find(fun div -> div.HasAttribute("class", "summaryPrice"))
                        |> fun div -> div.Descendants("span")                       
    
    let price = 
        try
            summaryPrice |> Seq.head |> fun span -> span.InnerText() 
                             |> fun str -> str.Replace(" ", "").Replace(" zł","")
                             |> fun str -> str.Replace("&nbsp;","")
                             |> System.Decimal.Parse |> LanguagePrimitives.DecimalWithMeasure<PLN>
        with 
            | :? System.FormatException -> raise(IncorrectAdvert(md5))

    let pricePerMeter = summaryPrice |> Seq.item(1) |> fun span -> span.InnerText() 
                             |> fun str -> (str.Split[|' '|]) |> Seq.head
                             |> System.Decimal.Parse |> LanguagePrimitives.DecimalWithMeasure<PLN/m^2>

    let listItems = html.Descendants("li") |> (Seq.map(fun li -> (li.Descendants("span")) |> Seq.tryHead))
                    |> Seq.filter(fun span -> span.IsSome)
                    |> Seq.map(fun span -> span.Value)
                    |> Seq.map(fun span -> (span.InnerText(), span.Descendants("em") |> Seq.tryHead))
                    |> Seq.filter(fun pair -> (snd(pair).IsSome))
                    |> Seq.map(fun pair -> (fst(pair), snd(pair).Value))


    let area = seekMorizonList(listItems, "Powierzchnia")
                    |> fun text -> (text.Split[|' '|]).[0]
                    |> System.Decimal.Parse |> LanguagePrimitives.DecimalWithMeasure<m^2>

    let roomsNr = trySeekMorizonListForInt(listItems, "Liczba pokoi")
    let tier = trySeekMorizonList(listItems, "Piętro")
    
    let buildingType = trySeekMorizonList(listItems, "Typ budynku")    
    let heating = trySeekMorizonList(listItems, "Ogrzewanie")
    let basement = trySeekMorizonListForBoolean(listItems, "Piwnica")
    let balcony = trySeekMorizonListForBoolean(listItems, "Balkon")
    let elevator = trySeekMorizonListForBoolean(listItems, "Winda")
    let furnished = trySeekMorizonListForBoolean(listItems, "Meble")

    let year = trySeekMorizonListForInt(listItems, "Rok budowy")

    let newConstruction = if year.IsSome && year.Value >= DateTime.Now.Year then Some true else Some false
     
    {
        Title = title; Description = description; 
        Md5 = md5; Url = link;
        TotalPrice = price; PricePerMeter = pricePerMeter; 
        Area = area; NumberOfRooms = roomsNr;
        Furnished = furnished; NewConstruction = newConstruction;
        BuildingType = buildingType; Tier = tier;
        YearOfConstruction = year;
        Elevator = elevator; Basement = basement;
        Balcony = balcony; Heating = heating;
        Parking = None; District = None; Street = None;
    }