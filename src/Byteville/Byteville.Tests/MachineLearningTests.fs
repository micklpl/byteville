namespace Byteville.Tests
open Byteville.MachineLearning
open Xunit

type MachineLearningTests() =

    [<Fact>]
    member x.LinearRegressionTest() = 
        let path = @"C:\mydir\Projekty\ByteVIlle\src\DataStorage\adverts.csv"
        let data = Byteville.MachineLearning.loadCsvData(path) 

        let coefficients = Byteville.MachineLearning.findCoefficients(data) |> Array.ofSeq

        Assert.NotEmpty(coefficients)