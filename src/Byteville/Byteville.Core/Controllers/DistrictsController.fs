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

    let GetElasticClient() = 
        let node = new Uri("http://localhost:9200")
        let settings = new ConnectionSettings(node)        
        new ElasticClient(settings.DefaultIndex("adverts"))

    member x.Get() =      
        let client = GetElasticClient()

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

        client.Search<AdministrationUnit>(searchRequest).Aggs.Terms("districts_by_popularity").Buckets.ToArray()
        |> Array.map(fun d -> { name = d.Key; streetsCount = int32(d.DocCount.Value); })

    member self.Get(id: string)  =  
        let client = GetElasticClient()

        let searchRequest = new SearchRequest()
        let termQuery = TermQuery()
        let field = new Field()
        field.Name <- "District"
        termQuery.Field <- field
        termQuery.Value <- id
        searchRequest.Query <- new QueryContainer(termQuery)
        searchRequest.Size <- System.Nullable<int>(300)
         
        client.Search<AdministrationUnit>(searchRequest).Documents.ToArray()
        |> Array.map(fun doc -> doc.Name)