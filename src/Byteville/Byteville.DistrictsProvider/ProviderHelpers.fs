[<AutoOpen>]
module ProviderHelpers

open FSharp.Data
open ProviderImplementation.ProvidedTypes

let cityFromBaseString city =
    match city with
        | "Kraków" -> "Krakowa"
        | "Warszawa" -> "Warszawy"
        | "Szczecin" -> "Szczecina"
        | "Wrocław" -> "Wrocławia"
        | "Katowice" -> "Katowic"
        | "Poznań" -> "Poznania"
        | "Gdańsk" -> "Gdańska"
        | "Opole" -> "Opola"
        | "Olsztyn" -> "Olsztyna"
        | _ -> ""


let tableRowToMap(trs:seq<HtmlNode>) = 
    let header = trs |> Seq.map(fun tr -> tr.Descendants("th"))
                     |> Seq.find(fun th -> (th |> Seq.length > 0))
                     |> Seq.map(fun th -> th.InnerText())

    trs |> Seq.map(fun tr -> (tr.Descendants("a") |> Seq.tryHead, 
                                tr.Descendants("td") 
                                |> Seq.map(fun t -> t.InnerText())))
        |> Seq.filter(fun pair -> fst(pair) |> fun a -> a.IsSome)
        |> Seq.map(fun p -> (fst(p)
                                |> fun a-> a.Value.InnerText(), 
                                            snd(p) |> Seq.zip header))
        |> Map.ofSeq

    

let getDistricts(city) = 
    let city = city |> cityFromBaseString
    HtmlDocument.Load("https://pl.wikipedia.org/wiki/Podzia%C5%82_administracyjny_"+ city)
        .Descendants("table") |> Seq.head
        |> fun item -> item.Descendants("tr")
        |> tableRowToMap

