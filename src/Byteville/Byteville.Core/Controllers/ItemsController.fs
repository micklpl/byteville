namespace Byteville.Core.Controllers
open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open Newtonsoft.Json

[<CLIMutable>]
type Item = {name: string; id: int }

type ItemsController() =
    inherit ApiController()
    static let values = new List<Item>([{name="car"; id=0}; {name="phone"; id=1}]) //mutable

    member self.Get() = values.ToArray()

    member self.Get(id) : IHttpActionResult =
        if id > values.Count - 1 then
            self.BadRequest() :> _
        else self.Ok(values.[id]) :> _

    member self.Post(item : Item) : IHttpActionResult =
        let newItem = {name=item.name; id= values.Count}
        values.Add(newItem)
        self.Created(String.Format("/api/items/{0}/", id), newItem) :>_

    member self.Delete(id) : IHttpActionResult = 
        let item = values.[id]
        values.Remove(item) |> ignore
        self.Ok() :>_

    member self.Put(item: Item, id) : IHttpActionResult = 
        if id < values.Count then 
            values.First(fun d -> d.id = id) |> values.Remove |> ignore        
        values.Add(item)
        self.Ok() :>_