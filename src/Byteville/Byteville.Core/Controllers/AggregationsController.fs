namespace Byteville.Core.Controllers
open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open Newtonsoft.Json
open Byteville.Core
open Nest
open Nest.Aggregations.Visitor
open Elasticsearch.Net
open Byteville.Core.Models
open System.Runtime.InteropServices

type District = {bucket: string; count: int }
type AdvertBase = {District: string}

[<AllowNullLiteral>]
type QueryModel() =
    member val firstThreshold = 0 with get, set
    member val lastThreshold = 0 with get, set
    member val size = 0 with get, set
    member val step = 0 with get, set
    member val statsField:string = null with get, set


type AggregationsController(client:ElasticClient) =
    inherit ApiController()

    member val Client = client with get,set

    member x.TermsAggregation(fieldName, size, statsField) = 
        let aggregator = new Nest.TermsAggregation(fieldName)
        let field = new Field()
        field.Name <- fieldName

        aggregator.Field <- field
        aggregator.Size <- new Nullable<int>(size)

        let container = new AggregationContainer()
        container.Terms <- aggregator
               
        container

    member x.RangeAggregation(fieldName, ranges) = 
        let aggregator = new Nest.RangeAggregation(fieldName)
        let field = new Field()
        field.Name <- fieldName

        aggregator.Field <- field 
        aggregator.Ranges <- ranges

        let container = new AggregationContainer()
        container.Range <- aggregator
               
        container

    member x.CreateStatsAggregation(statsField) =
        let dictionary = new Dictionary<string, IAggregationContainer>()
        let innerContainer = new AggregationContainer()
        let field = new Field()
        field.Name <- statsField
        innerContainer.Stats <- new Nest.StatsAggregation(statsField, field)
        dictionary.Add("inner_aggregation", innerContainer)
        AggregationDictionary(dictionary)

    member x.CreateRange(low:Option<int>, high:Option<int>) =
        let range = new Nest.Range()
        if low.IsSome then
            range.From <- new Nullable<float>(float(low.Value))
        if high.IsSome then
            range.To <- new Nullable<float>(float(high.Value))
        range

    member x.Ranges(first, last, step) =
        seq{
            yield x.CreateRange(None, Some(first)) :> IRange
            for thr in first .. step .. (last-step) 
                do yield x.CreateRange(Some(thr), Some(thr+step)) :> IRange
            yield x.CreateRange(Some(last), None) :> IRange
        }
    
    member x.Get(field: string, [<FromUri>]query:QueryModel) : IHttpActionResult =   
                 
        let searchRequest = new SearchRequest()
        let dictionary = new Dictionary<string, IAggregationContainer>()

        let container = match (query.firstThreshold, query.lastThreshold, query.step) with
                                    | (0,0,0) -> x.TermsAggregation(field, query.size, query.statsField) 
                                    | (f,l,s) -> x.RangeAggregation(field, x.Ranges(f,l,s))

        if not(query.statsField = null) then            
            container.Aggregations <- x.CreateStatsAggregation(query.statsField)

        dictionary.Add("outer_aggregation", container)

        searchRequest.Aggregations <- new AggregationDictionary(dictionary)

        let aggs = x.Client.Search<AdvertBase>(searchRequest).Aggs

        match aggs.Terms("outer_aggregation").Buckets.ToArray() with
                | [||] -> x.Ok(aggs.Range("outer_aggregation").Buckets.ToArray()) :> _
                | terms -> x.Ok(terms) :> _