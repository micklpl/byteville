namespace Byteville.Core.Controllers
open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open Newtonsoft.Json
open Byteville.Core
open Nest
open Byteville.Core.Models
open System.Configuration

type SearchController() =
    inherit ApiController()

    let mutable reqCount = 0

    let BuildPrefixQuery(phrase: String) = 
        let prefixQuery = new PrefixQuery()
        prefixQuery.Value <- phrase
        prefixQuery

    let BuildFuzzyQuery(phrase: String) = 
        let fuzzyQuery = new FuzzyQuery()
        fuzzyQuery.Value <- phrase
        fuzzyQuery

    let BuildQuery(name: String, phrase: String, fuzzy: bool) = 
        let req = new SearchRequest()
        let queryBase = match fuzzy with
                        | true -> BuildFuzzyQuery(phrase) :> FieldNameQueryBase
                        | false -> BuildPrefixQuery(phrase) :> FieldNameQueryBase
        
        let field = new Field()
        field.Name <- name
        queryBase.Field <- field
        req.Query <- new QueryContainer(queryBase)
        req

    let GetElasticClient(index) = 
        let node = new Uri(ConfigurationSettings.AppSettings.["ElasticsearchUri"])
        let settings = new ConnectionSettings(node)        
        new ElasticClient(settings.DefaultIndex(index))

    member self.PrefixSearch(q:String) = 
        let client = GetElasticClient("streets")
        let prefixQuery = BuildQuery("name", q, false)
        reqCount <- reqCount + 1
        client.Search<AdministrationUnit>(prefixQuery).Documents.ToArray()

    member x.Count() =
        reqCount

    member self.Get([<FromUri>]q :String) =
        let client = GetElasticClient("streets")
        let prefixQuery = BuildQuery("name", q, false)
        let search = fun (client: ElasticClient, query: SearchRequest) -> 
            client.Search<AdministrationUnit>(query).Documents.ToArray()
       
        match search(client, prefixQuery) with
                | [||] -> search(client, BuildQuery("name", q, true))
                | documents -> documents