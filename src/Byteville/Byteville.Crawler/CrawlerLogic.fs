module Byteville.CrawlerLogic
open System
open System.Net
open System.IO
open FSharp.Data
open System.Text.RegularExpressions

let olxAdverts = 
    seq { 
        let urlBase = "http://olx.pl/nieruchomosci/mieszkania/sprzedaz/krakow/q-mieszkania/"
        yield urlBase

        for page in 2 .. 20 do
            yield urlBase + "?page=" + page.ToString()
    }

let morizonAdverts = 
    seq { 
        let urlBase = "http://www.morizon.pl/mieszkania/krakow/"
        yield urlBase

        for page in 2 .. 200 do
            yield urlBase + "?page=" + page.ToString()
    }

let gumtreeAdverts = 
    seq {
        let urlBase = "http://www.gumtree.pl/s-mieszkania-i-domy-sprzedam-i-kupie/krakow/mieszkanie/"
        let checksum = "v1c9073l3200208a1dwp"
        yield urlBase + checksum + "1"

        let createUrl = fun i -> String.Format("{0}page-{1}/{2}{1}", urlBase, i, checksum)

        for page in 2 .. 200 do
            yield createUrl(page)
    }

let useTor = false

let downloadHtmlAsync(url:string) =
    async {
            let req = WebRequest.Create(url)
            if useTor then
                req.Proxy <- new WebProxy("127.0.0.1:8118"); 
            req.Timeout <- 2000
            let! rsp = req.AsyncGetResponse()          
            return rsp.GetResponseStream()
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

let md5 (text : string) : string =
    let data = System.Text.Encoding.UTF8.GetBytes(text)
    use md5 = System.Security.Cryptography.MD5.Create()
    (System.Text.StringBuilder(), md5.ComputeHash(data))
    ||> Array.fold (fun sb b -> sb.Append(b.ToString("x2")))
    |> string

let basePath = @"C:/mydir/Projekty/ByteVIlle/src/DataStorage/adverts"
    
let saveFileToDisk(asyncStream:Async<Stream>, md5:string) =
    async{
        let path = String.Format("{0}/{1}.html", basePath, md5)
        let! htmlStream = asyncStream 
        use fileStream = System.IO.File.Create(path)
        use ms = new MemoryStream()
        htmlStream.CopyTo(ms)
        let array = ms.ToArray()

        do! fileStream.AsyncWrite(array, 0, array.Length)
    } 

let crawl pages = 
    async{
        let sites = [(olxAdverts |> Seq.take(pages), parseOlx); 
                    (morizonAdverts |> Seq.take(pages), parseMorizon); 
                    (gumtreeAdverts |> Seq.take(pages), parseGumtree)]
        
        sites 
            |> List.map(fun (links, parser) -> 
                (links |> Seq.map(fun link-> downloadHtmlAsync(link) |> parser) |> Async.Parallel)
                |> Async.RunSynchronously)
            |> Seq.collect(fun d-> (d |> Seq.concat))
            |> Seq.map(fun link ->(link, md5(link)))
            |> Seq.sortBy(fun tuple -> snd(tuple))
            |> Seq.map(fun tuple -> (downloadHtmlAsync(fst(tuple)), snd(tuple)))
            |> Seq.map(fun (stream, md5) -> saveFileToDisk(stream, md5))
            |> Async.Parallel
            |> Async.RunSynchronously
            |> ignore
    }
    