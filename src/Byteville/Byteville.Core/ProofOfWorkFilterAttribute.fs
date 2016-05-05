namespace Byteville.Core

open System
open System.Text
open System.Web.Http.Filters
open System.Web.Http.Controllers
open System.Net.Http
open System.Net
open System.Runtime.Caching
open Newtonsoft.Json
open System.Security.Cryptography;

type ProofOfWorkFilterAttribute()  =
    inherit ActionFilterAttribute()

    let createChallenge() =        
        let rng = new RNGCryptoServiceProvider();
        let buffer = Array.zeroCreate<byte> 32
        rng.GetBytes(buffer);
        Convert.ToBase64String(buffer)    

    let storeChallenge(challenge:string) = 
        let policy = new CacheItemPolicy();
        policy.AbsoluteExpiration <- DateTimeOffset.Now.AddMinutes(2.0)
        MemoryCache.Default.Set(challenge, true, policy)

    let sha256(msg:string) = 
        let msgBytes = Encoding.UTF8.GetBytes(msg)
        let sha256 = new SHA256Managed()
        sha256.ComputeHash(msgBytes) 
            |> BitConverter.ToString
            |> fun str -> str.Replace("-", "")

    let unauthorizedResponse() = 
        let response = new HttpResponseMessage()
        response.StatusCode <- HttpStatusCode.Unauthorized
        response

    override x.OnActionExecuting(actionContext:HttpActionContext) =      
        let (found, values) = actionContext.Request.Headers.TryGetValues("X-Proof-Of-Work")
        if found = false then
            let challenge = createChallenge()
            storeChallenge challenge

            actionContext.Response <- new HttpResponseMessage()
            actionContext.Response.StatusCode <- HttpStatusCode.PreconditionFailed
            actionContext.Response.Content <- new StringContent(challenge)

        else
            let value = Seq.head values 
            let challenge = value |> fun value -> value.Split([|'#'|]) |> Seq.head

            if MemoryCache.Default.Contains(challenge) then
                let hash = value |> sha256
                if not(hash.StartsWith("0000")) then
                    actionContext.Response <- unauthorizedResponse()
                else
                    MemoryCache.Default.Remove(challenge) |> ignore
            else
                actionContext.Response <- unauthorizedResponse() 
            
        ()

