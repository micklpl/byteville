[<AutoOpen>]
module ProviderHelpers

open FSharp.Data

type DistrictInfo = {
    ``Powierzchnia [ha]``: float;
    ``Liczba stałych mieszkańców`` : int;
    ``Zagęszczenie ludności [osób/km²]`` : float;
}

let tableRowToDistrictInfo(tr:seq<HtmlNode>) = 
    let item i = tr |> Seq.item(i) |> fun i -> i.InnerText()
    match tr |> Seq.length with
        | 5 -> Some { 
                ``Powierzchnia [ha]`` = 
                    item 2 |> System.Double.Parse;
                ``Liczba stałych mieszkańców`` = 
                    item 3 |> System.Int32.Parse;
                ``Zagęszczenie ludności [osób/km²]`` = 
                    item 4 |> System.Double.Parse;
                } 
        | _ -> None

    

let getDistricts(city) = 
    let doc = HtmlDocument.Load("https://pl.wikipedia.org/wiki/Podzia%C5%82_administracyjny_"+ city)
    doc.Descendants("table") |> Seq.head
        |> fun item -> item.Descendants("tr")
        |> Seq.map(fun tr -> (tr.Descendants("a") |> Seq.tryHead, 
                                tr.Descendants("td") |> tableRowToDistrictInfo))
        |> Seq.filter(fun pair -> fst(pair) |> fun a -> a.IsSome)
        |> Seq.map(fun pair -> (fst(pair) |> fun a-> a.Value.InnerText(), snd(pair).Value))
        |> Map.ofSeq

