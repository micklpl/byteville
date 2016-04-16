module Byteville.Crawler.Parsers.Olx
open System.IO
open FSharp.Data
open Byteville.Core.Models
open System
open Byteville.Crawler.Helpers
open FSharp.Data.UnitSystems.SI.UnitSymbols
open System.Text.RegularExpressions

let olxAdverts = 
    seq { 
        let urlBase = "http://olx.pl/nieruchomosci/mieszkania/sprzedaz/krakow/q-mieszkania/"
        yield urlBase

        for page in 2 .. 20 do
            yield urlBase + "?page=" + page.ToString()
    }

let parseOlx(streamAsync:Async<Stream>) = 
    async{
        let! stream = streamAsync
        let html = HtmlDocument.Load(stream)
        return html.Descendants["table"]
            |> Seq.filter(fun t -> t.HasAttribute("id", "offers_table"))
            |> Seq.head
            |> fun table -> table.Descendants["a"]
            |> Seq.map(fun a -> a.AttributeValue("href"))
            |> Seq.map(fun link -> (link.Split[|'#'|]).[0])
            |> Seq.filter(fun link -> link.StartsWith("http://olx.pl/oferta/"))
            |> Seq.distinct
    }

let parseOlxTable(trs:seq<HtmlNode * HtmlNode>, th:String, tag:String) = 
    trs |> Seq.find(fun tr -> fst(tr).InnerText().Contains(th))
        |> fun tuple -> snd(tuple).Descendants(tag) |> Seq.head
        |> fun td -> td.InnerText() |> fun str -> (str.Split[|' '|])
        |> Seq.filter(fun f -> not(String.IsNullOrWhiteSpace(f)))
        |> Seq.head |> fun str -> str.Replace(".", ",")
    
let dateRegex = "creationDate=([0-9]{4}-[0-9]{2}-[0-9]{2})"

let olxAdvertParser(html:HtmlDocument, link:String) =
    let md5 = md5(link)

    let noscriptSrc = html.Descendants("noscript") |> Seq.map(fun d -> d.Descendants("img") |> Seq.tryHead)
                        |> Seq.filter(fun img -> img.IsSome)
                        |> Seq.map(fun img -> img.Value.AttributeValue("src"))
                        |> Seq.filter(fun img -> Regex.IsMatch(img, dateRegex))
                        |> Seq.head


    let creationDateStr = (Regex.Matches(noscriptSrc, dateRegex).[0].Value.Split[|'='|]).[1]
    let creationDate = DateTime.Parse(creationDateStr)

    let title = html.Descendants("h1") |> Seq.head |> fun node -> node.InnerText()

    let description = html.Descendants(fun f-> f.AttributeValue("id") = "textContent")
                        |> Seq.head |> fun div -> div.Descendants("p") |> Seq.head
                        |> fun p -> p.InnerText()

    let price = html.Descendants(fun f-> f.AttributeValue("class") = "pricelabel tcenter")
                |> Seq.head |> fun  div -> div.Descendants("strong") |> Seq.head
                |> fun strong -> strong.InnerText() |> fun str -> str.Replace("zł", "")
                |> System.Decimal.Parse |> LanguagePrimitives.DecimalWithMeasure<PLN>

    let trs = html.Descendants("tr") |> Seq.map(fun tr -> 
        ((tr.Descendants("th") |> Seq.tryHead), (tr.Descendants("td") |> Seq.tryHead)))
                |> Seq.filter(fun tr -> (Option.isSome(fst(tr)) && Option.isSome(snd(tr))))
                |> Seq.map(fun tr -> (fst(tr).Value, snd(tr).Value))

    let pricePerMeter = parseOlxTable(trs, "Cena za m", "strong") |> System.Decimal.Parse 
                        |> LanguagePrimitives.DecimalWithMeasure<PLN/m^2>                        

    let area = parseOlxTable(trs, "Powierzchnia", "strong") |> System.Decimal.Parse 
                        |> LanguagePrimitives.DecimalWithMeasure<m^2>

    let roomsNr = parseOlxTable(trs, "Liczba pokoi", "a") |> System.Int32.Parse
    let furnished = match parseOlxTable(trs, "Umeblowane", "a") with
                    | "Tak" -> true
                    | _ -> false
                    
    let newConstruction = match parseOlxTable(trs, "Rynek", "a") with
                          | "Pierwotny" -> true
                          | _ -> false
                          
    let buildingType = parseOlxTable(trs, "Rodzaj zabudowy", "a") 
    
    let tier = 
        try
            Some(parseOlxTable(trs, "Poziom", "a"))
        with 
            |  :? System.Collections.Generic.KeyNotFoundException -> None

    {
        Title = title; Description = description; 
        Md5 = md5; Url = link; CreationDate = creationDate;
        TotalPrice = price; PricePerMeter = pricePerMeter; 
        Area = area; NumberOfRooms = Some roomsNr;
        Furnished = Some furnished; NewConstruction = Some newConstruction;
        BuildingType = Some buildingType; Tier = tier;
        YearOfConstruction = None; Elevator = None;
        Basement = None; Balcony = None;
        Heating = None; Parking = None;
        District = None; Street = None;
        Location = None;
    }

