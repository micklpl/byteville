// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    //let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    Async.RunSynchronously(Byteville.CrawlerLogic.crawl(10))
    //stopWatch.Stop()
    //printfn "%f" stopWatch.Elapsed.TotalMilliseconds
    0 // return an integer exit code
