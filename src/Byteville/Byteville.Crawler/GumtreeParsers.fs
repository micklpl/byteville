module Byteville.Crawler.Parsers.Gumtree
open System.IO
open FSharp.Data
open System
open System.Text.RegularExpressions
open Byteville.Core.Models
open Byteville.Crawler.Helpers
open FSharp.Data.UnitSystems.SI.UnitSymbols

let gumtreeAdverts = 
    seq {
        let urlBase = "http://www.gumtree.pl/s-mieszkania-i-domy-sprzedam-i-kupie/krakow/mieszkanie/"
        let checksum = "v1c9073l3200208a1dwp"
        yield urlBase + checksum + "1"

        let createUrl = fun i -> String.Format("{0}page-{1}/{2}{1}", urlBase, i, checksum)

        for page in 2 .. 200 do
            yield createUrl(page)
    }

let streamToString(stream:Stream) = 
    use reader  = new StreamReader(stream)
    reader.ReadToEnd()

let parseGumtree(streamAsync:Async<Stream>) =
    async{
        let! stream = streamAsync    
        let html = stream |> streamToString
        let pattern = "href=\"\/(.*)\">"
        let correctLinkPart = "a-mieszkania-i-domy-sprzedam-i-kupie"
        return Regex.Matches(html, pattern) 
            |> Seq.cast<Match> 
            |> Seq.map(fun regMatch -> regMatch.Groups.[1].Value)
            |> Seq.filter(fun link -> link.Contains(correctLinkPart))
            |> Seq.map(fun link -> "http://www.gumtree.pl/" + link)
    }

let gumtreeAdvertParser(html:HtmlDocument, link:String) =
    let md5 = md5(link)

    let title = html.Descendants("h1") |> Seq.head |> fun h1-> h1.Descendants("span") 
                 |> Seq.head |> fun node -> node.InnerText()

    let description = html.Descendants("div") |> Seq.find(fun div -> div.HasAttribute("class", "description"))
                       |> fun div -> div.Descendants("span") |> Seq.head |> fun span -> span.InnerText()

    let price = html.Descendants("span") |> Seq.find(fun div -> div.HasAttribute("class", "amount"))
                |> fun span -> span.InnerText().Replace(" zł","")
                |> System.Decimal.Parse |> LanguagePrimitives.DecimalWithMeasure<PLN>

    let listItems = html.Descendants("li") |> (Seq.map(fun li -> (li.Descendants("span"))))
                    |> Seq.filter(fun span -> Seq.length(span) = 2)
                    |> Seq.map(fun span -> (Seq.head(span).InnerText(), (span |> Seq.item(1)).InnerText()))

    let creationDate = listItems |> Seq.find(fun pair -> (fst(pair)).Contains("Data dodania")) 
                        |> fun pair -> snd(pair) |> System.DateTime.Parse

    let area = 
        try
            listItems |> Seq.find(fun pair -> (fst(pair)).Contains("Wielkość")) |> fun pair -> snd(pair)
                         |> System.Decimal.Parse |> LanguagePrimitives.DecimalWithMeasure<m^2>
        with
            | :? System.Collections.Generic.KeyNotFoundException -> raise(IncorrectAdvert(md5))

    let roomsNr = 
        try
            listItems |> Seq.find(fun pair -> (fst(pair)).Contains("Liczba pokoi")) |> fun pair -> snd(pair)
                            |> fun str -> (str.Split[|' '|]).[0]
                            |> System.Int32.Parse |> fun int -> Some(int)
        with
            | :? System.FormatException -> None

    let parking = 
        try
            listItems |> Seq.find(fun pair -> (fst(pair)).Contains("Parking")) |> fun pair -> Some(snd(pair))
        with 
            | :? System.Collections.Generic.KeyNotFoundException -> None
                         
    {
        Title = title; Description = description; 
        Md5 = md5; Url = link; CreationDate = creationDate; 
        TotalPrice = price; PricePerMeter = (price/area); 
        Area = area; NumberOfRooms = roomsNr;
        Furnished = None; NewConstruction = None;
        BuildingType = None; Tier = None;
        YearOfConstruction = None; Elevator = None;
        Basement = None; Balcony = None;
        Heating = None; Parking = parking;
        District = None; Street = None;
    }


