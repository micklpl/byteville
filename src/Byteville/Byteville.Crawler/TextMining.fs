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
    PricePerMeter : decimal<PLN/m^2>;
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
                        |> LanguagePrimitives.DecimalWithMeasure<PLN/m^2>                        

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
    let title = html.Descendants("h1") |> Seq.head |> fun h1-> h1.Descendants("strong") 
                 |> Seq.head |> fun node -> node.InnerText()

    let description = html.Descendants("div") |> Seq.find(fun div -> div.HasAttribute("id", "description"))
                      |> fun div -> div.InnerText()

    let summaryPrice = html.Descendants("div") 
                        |> Seq.find(fun div -> div.HasAttribute("class", "summaryPrice"))
                        |> fun div -> div.Descendants("span")                       

    let price = summaryPrice |> Seq.head |> fun span -> span.InnerText() 
                             |> fun str -> str.Replace(" ", "").Replace(" zł","")
                             |> fun str -> str.Replace("&nbsp;","")
                             |> System.Decimal.Parse |> LanguagePrimitives.DecimalWithMeasure<PLN>

    let pricePerMeter = summaryPrice |> Seq.item(1) |> fun span -> span.InnerText() 
                             |> fun str -> (str.Split[|' '|]) |> Seq.head
                             |> System.Decimal.Parse |> LanguagePrimitives.DecimalWithMeasure<PLN/m^2>

    let listItems = html.Descendants("li") |> (Seq.map(fun li -> (li.Descendants("span")) |> Seq.tryHead))
                    |> Seq.filter(fun span -> span.IsSome)
                    |> Seq.map(fun span -> span.Value)
                    |> Seq.map(fun span -> (span.InnerText(), span.Descendants("em") |> Seq.tryHead))
                    |> Seq.filter(fun pair -> (snd(pair).IsSome))
                    |> Seq.map(fun pair -> (fst(pair), snd(pair).Value))


    let area = listItems |> Seq.find(fun pair -> (fst(pair)).Contains("Powierzchnia użytkowa"))
                         |> fun pair -> ((snd(pair)).InnerText().Split[|' '|]).[0]
                         |> System.Decimal.Parse |> LanguagePrimitives.DecimalWithMeasure<m^2>

    let roomsNr = listItems |> Seq.find(fun pair -> (fst(pair)).Contains("Liczba pokoi"))
                            |> fun pair -> snd(pair).InnerText() |> System.Int32.Parse

    {
        Title = title; Description = description; 
        Md5 = md5; Url = link;
        TotalPrice = price; PricePerMeter = pricePerMeter; 
        Area = area; NumberOfRooms = roomsNr
    }

    
let gumtreeAdvertParser(html:HtmlDocument, link:String) =
    let md5 = CrawlerLogic.md5(link)

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

    let area = listItems |> Seq.find(fun pair -> (fst(pair)).Contains("Wielkość")) |> fun pair -> snd(pair)
                         |> System.Decimal.Parse |> LanguagePrimitives.DecimalWithMeasure<m^2>

    let roomsNr = listItems |> Seq.find(fun pair -> (fst(pair)).Contains("Liczba pokoi")) |> fun pair -> snd(pair)
                            |> fun str -> (str.Split[|' '|]).[0]
                            |> System.Int32.Parse 
                         
    {
        Title = title; Description = description; 
        Md5 = md5; Url = link;
        TotalPrice = price; PricePerMeter = (price/area); 
        Area = area; NumberOfRooms = roomsNr
    }

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
                | StartsWith "http://www.morizon.pl" -> morizonAdvertParser(html, ogUrl)
                | StartsWith "http://www.gumtree.pl" -> gumtreeAdvertParser(html, ogUrl)
                | _ -> raise(NoOgUrlException(ogUrl))
    }      