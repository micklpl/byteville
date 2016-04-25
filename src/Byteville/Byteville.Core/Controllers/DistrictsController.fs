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

type District2 = { name:string; streetsCount:int}

type DistrictsController(client:ElasticClient) =
    inherit ApiController()

    member val Client = client with get,set

    member self.Get() =

        let searchRequest = new SearchRequest()
        let dictionary = new Dictionary<string, IAggregationContainer>()

        let aggregator = new Nest.TermsAggregation("District")
        let field = new Field()
        field.Name <- "District"

        aggregator.Field <- field
        aggregator.Size <- new System.Nullable<int>(40)
        aggregator.ExecutionHint <- new System.Nullable<TermsAggregationExecutionHint>(TermsAggregationExecutionHint.GlobalOrdinals)

        let container = new AggregationContainer()
        container.Terms <- aggregator

        dictionary.Add("districts_by_popularity", container)
        searchRequest.Aggregations <- new AggregationDictionary(dictionary)

        self.Client.Search<AdvertBase>(searchRequest).Aggs.Terms("districts_by_popularity").Buckets.ToArray()
        |> Array.map(fun d -> { name = d.Key; streetsCount = int32(d.DocCount.Value); })

    member self.Get(id: string)  =

        let searchRequest = new SearchRequest()
        let termQuery = TermQuery()
        let field = new Field()
        field.Name <- "District"
        termQuery.Field <- field
        termQuery.Value <- id
        searchRequest.Query <- new QueryContainer(termQuery)
        searchRequest.Size <- System.Nullable<int>(300)
         
        self.Client.Search<AdministrationUnit>(searchRequest).Documents.ToArray()
        |> Array.map(fun doc -> doc.Name)