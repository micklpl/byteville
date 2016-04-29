namespace Byteville.Core.Controllers
open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open Newtonsoft.Json
open EdgeJs
open System.Reflection
open System.IO
open Nest
open Byteville.Core.Models
open System.Net

type PrintController() =
    inherit ApiController()

    static let getPdfMakeCode() = 
        let assembly = Assembly.GetExecutingAssembly()
        let name = "print.js"
        use stream = assembly.GetManifestResourceStream(name)
        use reader = new StreamReader(stream)
        reader.ReadToEnd()

    static member val PdfMakeCode = getPdfMakeCode() with get

    member private x.GetAdvertJson(md5:string) = 
        let client = new WebClient();
        client.Encoding <- System.Text.Encoding.UTF8
        client.DownloadString("http://localhost:9200/adverts/_search?q=Md5:" + md5);

    member x.Get([<FromUri>]md5:string) =
        let func = Edge.Func(PrintController.PdfMakeCode)
        let advert = x.GetAdvertJson(md5)      
        let edgeResp = func.Invoke(advert) |> Async.AwaitTask |> Async.RunSynchronously
        let bytes = edgeResp :?> byte []
        let response = new HttpResponseMessage();
        response.Content <- new StreamContent(new MemoryStream(bytes))
        response.Content.Headers.Add("Content-type", "application/pdf")
        response
        

   