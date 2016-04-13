namespace Byteville.Core

open System
open System.Net.Http
open System.Web
open System.Web.Http
open System.Web.Routing
open System.Web.Http.Tracing
open Newtonsoft.Json

type HttpRoute = {
    controller : string
    id : RouteParameter }

type Global() =
    inherit System.Web.HttpApplication() 

    static member RegisterWebApi(config: HttpConfiguration) =
        // Configure routing
        config.MapHttpAttributeRoutes()
        config.Routes.MapHttpRoute(
            "AggregationsApi",
            "api/{controller}/{field}/",
            { controller = "Aggregations"; id = RouteParameter.Optional}
        ) |> ignore

        config.Routes.MapHttpRoute(
            "DefaultApi", // Route name
            "api/{controller}/{id}", // URL with parameters
            { controller = "{controller}"; id = RouteParameter.Optional } // Parameter defaults
        ) |> ignore

        // Configure serialization
        config.Formatters.Remove(config.Formatters.XmlFormatter) |> ignore
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling <- NullValueHandling.Ignore
        GlobalConfiguration.Configuration.Services.Replace(typeof<ITraceWriter>, new NLogger());

        Global.InitializeData()
        
        // Additional Web API settings

    member x.Application_Start() =
        GlobalConfiguration.Configure(Action<_> Global.RegisterWebApi) 
        
    member x.Application_BeginRequest() =
        // Stop Caching in IE
        x.Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);

        // Stop Caching in Firefox
        x.Response.Cache.SetNoStore();           

    static member InitializeData() = 
        let dataLoader = new DataLoader()
        if not(dataLoader.IndexExists("streets")) then
            dataLoader.IndexStreets("")
        ()