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

type District = {name: string; streetsCount: int }

type DistrictsController() =
    inherit ApiController()

    member x.Get() =
        let node = new Uri("http://localhost:9200")
        let settings = new ConnectionSettings(node)        
        let client = new ElasticClient(settings.DefaultIndex("streets"))

        let searchRequest = new SearchRequest()
        let dictionary = new Dictionary<string, IAggregationContainer>()

        let aggregator = new Nest.TermsAggregation("district")
        let field = new Field()
        field.Name <- "district"

        aggregator.Field <- field
        aggregator.Size <- new System.Nullable<int>(40)
        aggregator.ExecutionHint <- new System.Nullable<TermsAggregationExecutionHint>(TermsAggregationExecutionHint.GlobalOrdinals)

        let container = new AggregationContainer()
        container.Terms <- aggregator

        dictionary.Add("districts_by_popularity", container)
        searchRequest.Aggregations <- new AggregationDictionary(dictionary)

        client.Search<AdministrationUnit>(searchRequest).Aggs.Terms("districts_by_popularity").Buckets.ToArray()
        |> Array.map(fun d -> { name = d.Key; streetsCount = int32(d.DocCount.Value); })
