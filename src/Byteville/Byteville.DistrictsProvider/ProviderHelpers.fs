[<AutoOpen>]
module ProviderHelpers

open FSharp.Data

let getDistricts(city) = 
    let doc = HtmlDocument.Load("https://pl.wikipedia.org/wiki/Podzia%C5%82_administracyjny_"+ city)
    doc.Descendants("table") |> Seq.head
        |> fun item -> item.Descendants("a")
        |> Seq.map(fun a -> a.InnerText())
