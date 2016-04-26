namespace Byteville.Core.Controllers
open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open Newtonsoft.Json
open EdgeJs

type PrintController() =
    inherit ApiController()

    member x.Get() =
        let func = Edge.Func("
                return function (data, cb) {
                    cb(null, 'Node.js ' + process.version + ' welcomes ' + data);
                }
            ")

        let pr = func.Invoke(".NET")
        let b = Async.AwaitTask pr |> Async.RunSynchronously

        b
   