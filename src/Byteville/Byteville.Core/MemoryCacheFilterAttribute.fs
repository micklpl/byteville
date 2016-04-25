namespace Byteville.Core

open System
open System.Text
open System.Web.Http.Filters
open System.Web.Http.Controllers
open System.Net.Http
open System.Runtime.Caching
open Newtonsoft.Json

type MemoryCacheFilterAttribute(key:string) =
    inherit ActionFilterAttribute()

    member val cacheKey = key with get,set

    override x.OnActionExecuting(actionContext:HttpActionContext) =
        let value = MemoryCache.Default.Get x.cacheKey

        if not(value = null) then            
            let content = new StringContent(value.ToString(), Encoding.UTF8, "application/json");
            actionContext.Response <- new HttpResponseMessage()
            actionContext.Response.Content <- content  
        ()

    override x.OnActionExecuted(actionContext:HttpActionExecutedContext) =
        let policy = new CacheItemPolicy();
        policy.AbsoluteExpiration <- DateTimeOffset.Now.AddHours(1.0); 
        let value = actionContext.Response.Content.ReadAsStringAsync().Result 
        MemoryCache.Default.Set(x.cacheKey, value, policy)
        ()