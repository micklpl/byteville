module Byteville.MachineLearning
open Accord.Statistics.Models.Regression.Linear
open Accord.MachineLearning.VectorMachines.Learning
open Accord.MachineLearning.VectorMachines
open Accord.Statistics.Kernels
open System

let loadCsvData(path) = 
    System.IO.File.ReadAllLines(path) |> Seq.skip 1 //skip headers

let parseCsvData(dataCsv:seq<string>) = 
    let dataCsv = dataCsv |> Seq.map(fun row -> row.Replace(".",","))
    let numData = dataCsv |> Seq.map(fun row -> 
                            (row.Split(';') |> Seq.map(System.Double.Parse)))

    let vectorDim = (numData |> Seq.head |> Seq.length) - 1
    let inputs = numData |> Seq.map(fun seq -> seq |> Seq.take(vectorDim) |> Array.ofSeq)
                         |> Array.ofSeq
    let outputs = numData |> Seq.map(fun seq -> seq |> Seq.last) |> Array.ofSeq
    (vectorDim, inputs, outputs)

let findLinearCoefficients(dataCsv:seq<string>) =     
    let (vectorDim, inputs, outputs) = parseCsvData(dataCsv)

    let target = new MultipleLinearRegression(vectorDim)
    let error = target.Regress(inputs, outputs)

    target.Coefficients |> Seq.ofArray


let createSvm(dataCsv:seq<string>) =
    let (vectorDim, inputs, outputs) = parseCsvData(dataCsv)

    //redukcja parametrow do minimum
    let inputs = inputs |> Array.map(fun arr -> [|arr.[1]; arr.[5]|]) //l.pok, id.dziel
    let vectorDim = 2

    let machine = new KernelSupportVectorMachine(new Polynomial(3), vectorDim);
    let learn = new SequentialMinimalOptimizationRegression(machine, inputs, outputs);

    let error = learn.Run();

    let testData = [|3.0; 12.0|]
    let fxy = machine.Compute(testData);
    machine