module Byteville.TextMining
open System.IO
open FSharp.Data
open System
open FSharp.Data.UnitSystems.SI.UnitSymbols

[<Measure>] type PLN

type Advert = {
    Title: String;
    Description: String;
    Md5 : String;
    Url: String;
    TotalPrice : decimal<PLN>;
    PricePerMeter : decimal<PLN>;
    Area : decimal<m^2>;
    NumberOfRooms : int
}

let loadFileFromDisk path = 
    async{
        use fileStream = File.OpenRead(path)
        let buff = Array.zeroCreate(int(fileStream.Length))
        let! bytes = fileStream.AsyncRead(buff)
        return new MemoryStream(buff) :> Stream
    }

let (|StartsWith|_|) needle (haystack : string) = if haystack.StartsWith(needle) then Some() else None

let parseOlxTable(trs:seq<HtmlNode * HtmlNode>, th:String, tag:String) = 
    trs |> Seq.find(fun tr -> fst(tr).InnerText().Contains(th))
        |> fun tuple -> snd(tuple).Descendants(tag) |> Seq.head
        |> fun td -> td.InnerText() |> fun str -> (str.Split[|' '|])
        |> Seq.filter(fun f -> not(String.IsNullOrWhiteSpace(f)))
        |> Seq.head |> fun str -> str.Replace(".", ",")
    

let olxAdvertParser(html:HtmlDocument, link:String) =
    let md5 = CrawlerLogic.md5(link)
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
                        |> LanguagePrimitives.DecimalWithMeasure<PLN>                        

    let area = parseOlxTable(trs, "Powierzchnia", "strong") |> System.Decimal.Parse 
                        |> LanguagePrimitives.DecimalWithMeasure<m^2>

    let roomsNr = parseOlxTable(trs, "Liczba pokoi", "a") |> System.Int32.Parse                        

    {
        Title = title; Description = description; 
        Md5 = md5; Url = link;
        TotalPrice = price; PricePerMeter = pricePerMeter; 
        Area = area; NumberOfRooms = roomsNr
    }
    
let morizonAdvertParser(html:HtmlDocument, link:String) =
    let md5 = CrawlerLogic.md5(link)
    md5
    
let gumtreeAdvertParser() = 
    3

exception NoOgUrlException of string
    
let classifyAdvert(asyncStream:Async<Stream>) = 
    async{
        let! stream = asyncStream
        let html = HtmlDocument.Load(stream)
        let ogUrl = html.Descendants("meta")
                    |> Seq.find(fun meta -> meta.HasAttribute("property", "og:url"))
                    |> fun node -> node.Attribute("content").Value()

        return match ogUrl with
                    | StartsWith "http://olx.pl" -> olxAdvertParser(html, ogUrl)
                    | StartsWith "http://morizon.pl" -> raise(NoOgUrlException(ogUrl))
                    | StartsWith "http://gumtree.pl" -> raise(NoOgUrlException(ogUrl))
                    | _ -> raise(NoOgUrlException(ogUrl))
    }      