module Byteville.TextMining
open System.IO
open FSharp.Data
open System
open FSharp.Data.UnitSystems.SI.UnitSymbols
open System.Text.RegularExpressions

[<Measure>] type PLN

type Advert = {
    Title: String;
    Description: String;
    Md5 : String;
    Url: String;
    TotalPrice : decimal<PLN>;
    PricePerMeter : decimal<PLN/m^2>;
    Area : decimal<m^2>;
    NumberOfRooms : Option<int>;
    Furnished : Option<bool>;
    NewConstruction : Option<bool>;
    BuildingType : Option<string>;
    Tier : Option<string>;
    YearOfConstruction : Option<int>;
    Elevator : Option<bool>;
    Basement: Option<bool>;
    Balcony: Option<bool>;
    Heating: Option<string>;
    Parking: Option<string>;

    mutable Street: Option<string>;
    mutable District: Option<string>;
}

exception IncorrectAdvert of string

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
        Md5 = md5; Url = link;
        TotalPrice = price; PricePerMeter = pricePerMeter; 
        Area = area; NumberOfRooms = Some roomsNr;
        Furnished = Some furnished; NewConstruction = Some newConstruction;
        BuildingType = Some buildingType; Tier = tier;
        YearOfConstruction = None; Elevator = None;
        Basement = None; Balcony = None;
        Heating = None; Parking = None;
        District = None; Street = None;
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
    let md5 = CrawlerLogic.md5(link)
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
        Md5 = md5; Url = link;
        TotalPrice = price; PricePerMeter = (price/area); 
        Area = area; NumberOfRooms = roomsNr;
        Furnished = None; NewConstruction = None;
        BuildingType = None; Tier = None;
        YearOfConstruction = None; Elevator = None;
        Basement = None; Balcony = None;
        Heating = None; Parking = parking;
        District = None; Street = None;
    }

exception NoOgUrlException of string

let classifyAdvert(asyncStream:Async<Stream>) = 
    async{
        let! stream = asyncStream
        let html = HtmlDocument.Load(stream)
        let ogUrl = html.Descendants("meta")
                    |> Seq.find(fun meta -> meta.HasAttribute("property", "og:url"))
                    |> fun node -> node.Attribute("content").Value()

        try
            let value = match ogUrl with
                        | StartsWith "http://olx.pl" -> Some(olxAdvertParser(html, ogUrl))
                        | StartsWith "http://www.morizon.pl" -> Some(morizonAdvertParser(html, ogUrl))
                        | StartsWith "http://www.gumtree.pl" -> Some(gumtreeAdvertParser(html, ogUrl))
                        | _ -> raise(NoOgUrlException(ogUrl))
            return value
        with
            |  :? IncorrectAdvert -> return None
    }

let searchController = new Byteville.Core.Controllers.SearchController()

let neighboursPattern = "(ul|ulica|al|aleja|os|osiedle)"

let findNeighbourhoodWords(tokens:string[]) = 
    Array.FindIndex(tokens, fun token -> Regex.IsMatch(token, neighboursPattern))

let tryGetItem(tokens:string[], index:int) =
    if index >= tokens.Length then 
        None 
    else 
        let item = Array.item(index) <| tokens
        if item.Length > 2 then Some(item) else None 

let tryParseStreetByNeighbours tokens =     
    match findNeighbourhoodWords(tokens) with
        | -1 -> None
        | index -> tryGetItem(tokens, index + 1)

let tryParseStreet(title:String) = 
    let tokens = title.Replace(".","").Replace(",","").ToLower().Split[|' '|]
    match tryParseStreetByNeighbours(tokens) with
        | None -> None
        | Some(street) -> match searchController.Get(street) with
                                | [|x|] -> Some(x)
                                | _ -> None