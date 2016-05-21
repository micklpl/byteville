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
open Accord.Statistics.Models.Regression.Linear

[<AllowNullLiteral>]
type EstimationsQueryModel() =
    member val districtId = 0 with get, set
    member val tier = 0 with get, set
    member val numberOfRooms = 0 with get,set
    member val area = 0.0 with get, set
    member val parking = false with get, set
    member val yearOfConstruction = 2000 with get, set

type EstimationsController() =
    inherit ApiController()

    static let LoadAdvertsCsv() = 
        let assembly = Assembly.GetExecutingAssembly()
        let name = "adverts.csv"
        use stream = assembly.GetManifestResourceStream(name)
        use reader = new StreamReader(stream)
        reader.ReadToEnd().Split([|'\n'|]) |> Seq.skip 1

    static let BuildRegression(dim,data:seq<seq<float>>) = 
        let regression = new MultipleLinearRegression(dim)
        let outputs = data |> Seq.map(fun line -> line |> Seq.last) |> Array.ofSeq
        let inputs = data |> Seq.map(fun line -> line |> Seq.take(dim) |> Array.ofSeq) |> Array.ofSeq
        regression.Regress(inputs, outputs) |> ignore
        regression

    static let CreateRegressionModels() = 
        let data = LoadAdvertsCsv()
                       |> Seq.map(fun row -> row.Replace(".",","))
                       |> Seq.map(fun line -> line.Split(';') |> Seq.map(System.Double.Parse))
        
        let vectorDim = (data |> Seq.head |> Seq.length) - 2

        data |> Seq.groupBy(fun line -> line |> Seq.nth(5))
             |> Seq.sortBy(fun gr -> fst(gr))
             |> Seq.map(fun districtData -> BuildRegression(vectorDim, snd(districtData)))

    static member val RegressionModels = CreateRegressionModels() with get

    member x.Get([<FromUri>]q:EstimationsQueryModel) : IHttpActionResult =
        let regression = EstimationsController.RegressionModels |> Seq.nth(q.districtId)
        let parking = if q.parking then 1.0 else 0.0
        let parameters = [|
                            q.area; 
                            float q.numberOfRooms; 
                            float q.tier;
                            parking;
                            float q.yearOfConstruction
                         |]
        x.Ok(regression.Compute(parameters)) :> IHttpActionResult