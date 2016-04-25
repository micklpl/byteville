namespace Byteville.Core.Controllers
open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open Newtonsoft.Json
open Byteville.Core
open Nest

type TrendsController(client:ElasticClient) =
    inherit ApiController()

    let LastMonthFilter() =
        let monthAgo = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd")
        let datesRange = new Nest.DateRangeQuery()
        datesRange.GreaterThanOrEqualTo <- DateMath.FromString(monthAgo)
        let field = new Field()
        field.Name <- "CreationDate"
        datesRange.Field <- field
        new QueryContainer(datesRange)

    let DescriptionAggregation() =
        let dictionary = new Dictionary<string, IAggregationContainer>()
        let aggregator = new Nest.TermsAggregation("Description")
        
        let field = new Field()
        field.Name <- "Description"

        aggregator.Field <- field
        aggregator.Size <- new System.Nullable<int>(15)

        let container = new AggregationContainer()
        container.Terms <- aggregator
        
        dictionary.Add("Description", container)
        new AggregationDictionary(dictionary)

    member val Client = client with get,set

    [<MemoryCacheFilter("trends")>]
    member x.Get() : IHttpActionResult = 
        let query = new SearchRequest()
        query.Query <- LastMonthFilter()
        query.Aggregations <- DescriptionAggregation()

        x.Client.Search<AdvertBase>(query).Aggregations |> x.Ok :> IHttpActionResult