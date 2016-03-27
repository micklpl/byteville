// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    Async.RunSynchronously(Byteville.CrawlerLogic.crawl(1))
    0 // return an integer exit code
