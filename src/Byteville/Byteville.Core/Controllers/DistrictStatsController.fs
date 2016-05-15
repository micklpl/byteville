namespace Byteville.Core.Controllers
open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open Newtonsoft.Json
open Byteville.Core
open System.Reflection
open System.IO

type DistrictStatsController() =
    inherit ApiController()

    static let GetDistrictsCsv() = 
        let assembly = Assembly.GetExecutingAssembly()
        let name = "districts.csv"
        use stream = assembly.GetManifestResourceStream(name)
        use reader = new StreamReader(stream)
        reader.ReadToEnd().Split([|'\n'|])

    static let MapDistrict(key) =
        match key with
            | "Bieżanów-Prokocim" -> "Prokocim-Bieżanów"
            | "Podgórze Duchackie" -> "Wola Duchacka"
            | "Łagiewniki-Borek Fałęcki" -> "Łagiewniki"
            | _ -> key

    static let GetDistricts() = 
        let csv = GetDistrictsCsv()
        let header = csv.First().Split([|';'|]).Skip 1
        let districts = csv.Skip(1) 
                            |> Seq.map(fun line -> line.Split([|';'|])) 
                            |> Seq.filter(fun line -> line.Count() = 4)

        header 
            |> Seq.mapi(
                fun i elem -> (elem, districts 
                                        |> Seq.map(fun line -> (MapDistrict(line.[0]), line.[i+1])) 
                                        |> Map.ofSeq))
            |> Map.ofSeq

    static member val DistrictStats = GetDistricts() with get

    member self.Get() =
        self.Unauthorized()

    member self.Get(id: string)  =
        DistrictStatsController.DistrictStats.[id] |> JsonConvert.SerializeObject
        