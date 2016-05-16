module Byteville.CrawlerLogic
open System
open System.Net
open System.IO
open FSharp.Data
open System.IO.Compression
open Byteville.Crawler.Parsers.Olx
open Byteville.Crawler.Parsers.Morizon
open Byteville.Crawler.Parsers.Gumtree
open Byteville.Crawler.Helpers

let useTor = false

let downloadHtmlAsync(url:string) =
    async {
            let req = WebRequest.Create(url)
            let gzip = url.StartsWith("http://www.gumtree.pl")
            if useTor then
                req.Proxy <- new WebProxy("127.0.0.1:8118"); 

            req.Timeout <- 2000

            if gzip then
                req.Headers.Add("Accept-Encoding", "gzip")
                            
            let! rsp = req.AsyncGetResponse()
            let mutable stream = rsp.GetResponseStream()
            
            if gzip then
                stream <- new GZipStream(stream, CompressionMode.Decompress)
                    
            return stream
          }

let basePath = @"C:/mydir/Projekty/ByteVIlle/src/DataStorage/adverts"
    
let saveFileToDisk(asyncStream:Async<Stream>, md5:string) =
    async{
        let path = String.Format("{0}/{1}.html", basePath, md5)
        let! htmlStream = asyncStream 
        use fileStream = System.IO.File.Create(path)
        use ms = new MemoryStream()
        htmlStream.CopyTo(ms)
        let array = ms.ToArray()

        try
            do! fileStream.AsyncWrite(array, 0, array.Length)
        with
            | :? System.IO.IOException -> ()
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
            |> Seq.distinct
            |> Seq.map(fun tuple -> (downloadHtmlAsync(fst(tuple)), snd(tuple)))
            |> Seq.map(fun (stream, md5) -> saveFileToDisk(stream, md5))
            |> Async.Parallel
            |> Async.RunSynchronously
            |> ignore
    }