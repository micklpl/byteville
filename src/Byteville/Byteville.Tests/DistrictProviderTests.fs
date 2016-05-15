namespace Byteville.Tests 
open Xunit
open Byteville.DistrictProvider
open System.Text

type KrakowDistricts = DistrictProvider<"Kraków">

type DistrictProviderTests() =

    member x.ExtractDistrictName(str:string) =
        let words = str.Split([|' '|]) |> Seq.skip 2
        System.String.Join(" ", words)

    member x.PrintMap(map:Map<string,string>) = 
        new StringBuilder()
            |> fun sb -> sb.Append(map |> Map.find("Nazwa dzielnicy") |> x.ExtractDistrictName)
            |> fun sb -> sb.Append(";")
            |> fun sb -> sb.Append(map |> Map.find("Powierzchnia\r\n [ha]"))
            |> fun sb -> sb.Append(";")
            |> fun sb -> sb.Append(map |> Map.find("Liczba stałych\r\n mieszkańców"))
            |> fun sb -> sb.Append(";")
            |> fun sb -> sb.Append(map |> Map.find("Zagęszczenie ludności [osób/km²]"))
            |> fun sb -> sb.Append("\n")
            |> fun sb -> sb.ToString()

    [<Fact>]
    member x.DistrictStatsToCsv() =
        let prov = new KrakowDistricts()
        let path = @"C:\mydir\Projekty\ByteVIlle\src\DataStorage\districts.csv"
        let header = "nazwa;powierzchnia;liczba_mieszkancow;zageszczenie_ludnosci\n"

        use wr = new System.IO.StreamWriter(path, false, System.Text.Encoding.UTF8)

        wr.Write header
        prov.``Dzielnica I Stare Miasto`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica II Grzegórzki`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica III Prądnik Czerwony`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica IV Prądnik Biały`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica V Krowodrza`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica VI Bronowice`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica VII Zwierzyniec`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica VIII Dębniki`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica IX Łagiewniki-Borek Fałęcki`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica X Swoszowice`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica XI Podgórze Duchackie`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica XII Bieżanów-Prokocim`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica XIII Podgórze`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica XIV Czyżyny`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica XV Mistrzejowice`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica XVI Bieńczyce`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica XVII Wzgórza Krzesławickie`` |> x.PrintMap |> wr.Write
        prov.``Dzielnica XVIII Nowa Huta`` |> x.PrintMap |> wr.Write

        wr.Close()

        Assert.True(true)



