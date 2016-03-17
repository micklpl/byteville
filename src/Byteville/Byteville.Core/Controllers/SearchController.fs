namespace Byteville.Core.Controllers
open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open Newtonsoft.Json
open Byteville.Core

type SearchController() =
    inherit ApiController()

    member self.Get() = self.Ok()
