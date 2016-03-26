﻿module Byteville.CrawlerLogic
open System
open System.Net
open System.IO
open FSharp.Data

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

        let createUrl = fun i -> String.Format("{0}page-{1}/v1c9073l3200208a1dwp{1}", urlBase, i)

        for page in 2 .. 200 do
            yield createUrl(page)
    }

let downloadHtmlAsync(url:string) =
    async {
            let req = WebRequest.Create(url)
            let! rsp = req.AsyncGetResponse()
            return rsp.GetResponseStream()
          }

let parseOlx(stream:Stream) = 
    let html = HtmlDocument.Load(stream)
    html.Descendants["table"]
        |> Seq.filter(fun t -> t.HasAttribute("id", "offers_table"))
        |> Seq.head
        |> fun table -> table.Descendants["a"]
        |> Seq.map(fun a -> a.AttributeValue("href"))
        |> Seq.map(fun link -> (link.Split[|'#'|]).[0])
        |> Seq.filter(fun link -> link.StartsWith("http://olx.pl/oferta/"))
        |> Seq.distinct
//        |> fun item -> item.ToString