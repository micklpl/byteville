namespace Byteville.Core.Controllers
open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open Newtonsoft.Json
open Byteville.Core
open Nest

type ScoredAdvertMetadata = {
    AdvertMetadata: AdvertMetadata;
    Score: float;
    }

[<AllowNullLiteral>]
type RecommendationsQueryModel() =
    member val lat:float = 0.0 with get, set
    member val lon:float = 0.0 with get, set
    member val price:float = 300000.0 with get, set
    member val area:float = 50.0 with get, set

type RecommendationsController() =
    inherit ApiController()

    let GetElasticClient(index) = 
        let node = new Uri("http://localhost:9200")
        let settings = new ConnectionSettings(node)        
        new ElasticClient(settings.DefaultIndex(index))

    let CreateNameField(name) =
        let field = new Field()
        field.Name <- name
        field

    let CreateExistsFilter() =
        let existsQuery = new Nest.ExistsQuery()
        existsQuery.Field <- CreateNameField("Location")
        new QueryContainer(existsQuery)

    let GeoDistanceQuery(lat, lon) =         
        let distGauss = new Nest.GaussGeoDecayFunction()

        let offset = new Nest.Distance("2km")
        let scale = new Nest.Distance("5km")
        let loc = new Nest.GeoLocation(lat, lon)

        distGauss.Field <- CreateNameField("Location")
        distGauss.Offset <- offset
        distGauss.Scale <- scale
        distGauss.Origin <- loc 
        distGauss.Weight <- new Nullable<float>(10.0)    

        distGauss

    let PriceLinearQuery(price) =         
        let priceLin = new Nest.GaussDecayFunction()

        priceLin.Field <- CreateNameField("TotalPrice")
        priceLin.Offset <- new Nullable<float>(0.1 * price)
        priceLin.Scale <- new Nullable<float>(0.25 * price)
        priceLin.Origin <- new Nullable<float>(0.95 * price)
        priceLin.Weight <- new Nullable<float>(2.0) 

        priceLin

    let AreaLinearQuery(area) =         
        let areaLin = new Nest.LinearDecayFunction()

        areaLin.Field <- CreateNameField("Area")
        areaLin.Offset <- new Nullable<float>(5.0)
        areaLin.Scale <- new Nullable<float>(10.0)
        areaLin.Origin <- new Nullable<float>(area)
        areaLin.Weight <- new Nullable<float>(5.0)         

        areaLin

    member x.Get([<FromUri>]query:RecommendationsQueryModel) = 
        let client = GetElasticClient("adverts")
        let req = new SearchRequest()

        let score = new Nest.FunctionScoreQuery()
        let functions = new List<IScoreFunction>()

        GeoDistanceQuery(query.lat, query.lon) |> functions.Add
        PriceLinearQuery(query.price) |> functions.Add
        AreaLinearQuery(query.area) |> functions.Add
        score.Functions <- functions
       
        req.Query <- new QueryContainer(score)
        req.MinScore <- new Nullable<float>(0.00)

        let result = client.Search<AdvertMetadata>(req).Hits.ToArray() 
                        |> Array.map(fun hit -> {AdvertMetadata = hit.Source; 
                                                    Score = hit.Score})

        x.Ok(result) :> IHttpActionResult