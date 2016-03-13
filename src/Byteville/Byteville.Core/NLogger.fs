namespace Byteville.Core
open NLog
open System.Web.Http.Tracing
open System.Collections.Generic
open System
open System.Net.Http
open System.Text

type NLogger() =    
    static let mappingDictionary = new Dictionary<TraceLevel, Action<string>>()
    static do
        let classLogger = LogManager.GetCurrentClassLogger()
        mappingDictionary.Add(TraceLevel.Info, Action<string> classLogger.Info)
        mappingDictionary.Add(TraceLevel.Debug, Action<string> classLogger.Debug)
        mappingDictionary.Add(TraceLevel.Error, Action<string> classLogger.Error)
        mappingDictionary.Add(TraceLevel.Fatal, Action<string> classLogger.Fatal)
        mappingDictionary.Add(TraceLevel.Warn, Action<string> classLogger.Warn)

    static let loggingMap = new Lazy<Dictionary<TraceLevel, Action<string>>>(fun () -> mappingDictionary)

    member x.Log (record: TraceRecord) = 
            let message = new StringBuilder();
            if record.Request <> null then
                if record.Request.Method <> null then
                    message.Append(record.Request.Method) |> ignore
                if record.Request.RequestUri <> null then
                    message.Append(" ").Append(record.Request.RequestUri) |> ignore

            if not(String.IsNullOrWhiteSpace record.Category) then
                message.Append(" ").Append(record.Category) |> ignore
            
            if not(String.IsNullOrWhiteSpace record.Operator) then
                message.Append(" ").Append(record.Operator).Append(" ").Append(record.Operation) |> ignore

            if not(String.IsNullOrWhiteSpace record.Message) then
                message.Append(" ").Append(record.Message) |> ignore

            if record.Exception <> null && 
                not(record.Exception.GetBaseException().Message |> String.IsNullOrWhiteSpace) then
                message.Append(" ").Append(record.Exception.GetBaseException().Message) |> ignore
            
            loggingMap.Value.[record.Level].Invoke(message.ToString())

    interface ITraceWriter with
        member x.Trace (request: HttpRequestMessage, category: string, level: TraceLevel, traceAction: Action<TraceRecord>)  =
            if level <> TraceLevel.Off then
                let record = new TraceRecord(request, category, level)
                traceAction.Invoke(record)
                x.Log(record)