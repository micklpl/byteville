module Byteville.MachineLearning
open Accord.Statistics.Models.Regression.Linear
open System

let loadCsvData(path) = 
    System.IO.File.ReadAllLines(path) |> Seq.skip 1 //skip headers

let findCoefficients(dataCsv:seq<string>) = 
    let dataCsv = dataCsv |> Seq.map(fun row -> row.Replace(".",","))
    let numData = dataCsv |> Seq.map(fun row -> 
                            (row.Split(';') |> Seq.map(System.Double.Parse)))

    let vectorDim = (numData |> Seq.head |> Seq.length) - 1
    let inputs = numData |> Seq.map(fun seq -> seq |> Seq.take(vectorDim) |> Array.ofSeq)
                         |> Array.ofSeq
    let outputs = numData |> Seq.map(fun seq -> seq |> Seq.last) |> Array.ofSeq

    let target = new MultipleLinearRegression(vectorDim)
    let error = target.Regress(inputs, outputs)

    target.Coefficients |> Seq.ofArray
