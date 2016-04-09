﻿namespace Byteville.Core.Controllers
open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open Newtonsoft.Json
open Byteville.Core
open Nest
open FSharp.Core.Operators.Unchecked

[<AllowNullLiteral>]
type FtsQueryModel() =
    member val q:string = null with get, set
    member val datesFrom:string = null with get, set
    member val datesTo:string = null with get, set
    member val priceFrom = defaultof<float> with get, set
    member val priceTo = defaultof<float> with get, set
    member val pricePerMeterFrom = defaultof<float> with get, set
    member val pricePerMeterTo = defaultof<float> with get, set
    member val district:string = null with get,set
    member val street:string = null with get,set

type AdvertMetadata =
    {
        TotalPrice: double;
        PricePerMeter: double;
        Area: double;
        NumberOfRooms: double;
        Street: string;
        District: string;
        Title : string;
        Md5: string
    }

type FilterInequality =
   | LTE of value: float
   | GTE of value: float

type FtsController() =
    inherit ApiController()

    let GetElasticClient(index) = 
        let node = new Uri("http://localhost:9200")
        let settings = new ConnectionSettings(node)        
        new ElasticClient(settings.DefaultIndex(index))

    let CreateNameField(name) =
        let field = new Field()
        field.Name <- name
        field

    let CreateTermsQuery(term: obj, name: string) = 
        let termQuery = new Nest.TermQuery()        
        termQuery.Field <- CreateNameField(name)
        termQuery.Value <- term
        new QueryContainer(termQuery)

    let CreateRangeQuery(inequality: FilterInequality, name) = 
        let range = new Nest.NumericRangeQuery()
        match inequality with
                | GTE(value) ->  range.GreaterThanOrEqualTo <- new Nullable<float>(value) 
                | LTE(value) ->  range.LessThanOrEqualTo <- new Nullable<float>(value)  

        range.Field <- CreateNameField(name)
        new QueryContainer(range)

    let CreateRangeQueries(low, high, name) =                  
        seq{
            match (low,high) with
                | (0.0, 0.0) -> ()
                | (0.0, y) -> yield CreateRangeQuery(LTE(y), name)
                | (x, 0.0) -> yield CreateRangeQuery(GTE(x), name)
                | (x,y) -> yield CreateRangeQuery(GTE(x), name); 
                           yield CreateRangeQuery(LTE(y), name)                                    
        }


    let BuildQuery(model:FtsQueryModel) = 
        let req = new SearchRequest()
        let boolQuery = new Nest.BoolQuery()
        let filters = new List<QueryContainer>()

        if not(model.district = null) then 
            CreateTermsQuery(model.district, "District") |> filters.Add

        if not(model.street = null) then
            CreateTermsQuery(model.street, "Street") |> filters.Add

        CreateRangeQueries(model.priceFrom, model.priceTo, "TotalPrice") |> filters.AddRange
        CreateRangeQueries(model.pricePerMeterFrom, model.pricePerMeterTo, "PricePerMeter") |> filters.AddRange

        boolQuery.Must <- filters        
        req.Query <- new QueryContainer(boolQuery)
        req

    member x.Get([<FromUri>]queryModel:FtsQueryModel) : IHttpActionResult = 
        let client = GetElasticClient("adverts")
        let query = BuildQuery(queryModel)
        let result = client.Search<AdvertMetadata>(query).Documents.ToArray()
        x.Ok(result) :> IHttpActionResult