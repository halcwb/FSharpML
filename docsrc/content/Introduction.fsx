(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
//#r "../../packages/formatting/FSharp.Plotly/lib/netstandard2.0/Fsharp.Plotly.dll"
//#r "netstandard"
//open FSharp.Plotly
(**
Sample Spam detection
=========================

**)

#load "../../bin/FSharpML/netstandard2.0/FSharpML.fsx"
//#load "../../FSharpML.fsx"

open System;
open Microsoft.ML
open Microsoft.ML.Data;
open FSharpML

(**
Introduction
============

FSharpML is a functional-friendly lightweight wrapper of the powerful ML.Net library. It is designed to enable users to explore ML.Net in a scriptable
manner, while maintaining the functional style of F#.

After installing the package via Nuget we can load the delivered reference script and start using ML.Net in conjunction with FSharpML.
*)
#load "../../bin/FSharpML/netstandard2.0/FSharpML.fsx"

open System;
open Microsoft.ML
open Microsoft.ML.Data;
open FSharpML

(**
To get a feel how this library handles ML.Net operations we rebuild the Spam Detection tutorial (https://github.com/dotnet/machinelearning-samples/tree/master/samples/fsharp/getting-started/BinaryClassification_SpamDetection)
given by ML.Net. 
We will start by instantiating a MLContext, the heart of the ML.Net API and intended to serve as a method catalog. We will 
now use it to set a scope on data stored in a text file. The method name might be misleading, but ML.Net readers are lazy and
the reading process will start when the data is processed. (https://github.com/dotnet/machinelearning/blob/master/docs/code/IDataViewTypeSystem.md#standard-column-types)
*)

let mlContext = MLContext(seed = Nullable 1)

let trainDataPath  = (__SOURCE_DIRECTORY__  + "./data/SMSSpamCollection.txt")
    
let data = 
    mlContext.Data.ReadFromTextFile( 
            path = trainDataPath,
            columns = 
                [|
                    TextLoader.Column("LabelText" , Nullable DataKind.Text, 0)
                    TextLoader.Column("Message" , Nullable DataKind.Text, 1)
                |],
            hasHeader = false,
            separatorChar = '\t')

(**
Now that we told our interactive environment about the data we can start thinking about a model (EstimatorChain in the ML.Net jargon)
we want to build. As the MLContext serves as a catalog we will use it to draw transformations that can be appended to form a estimator chain.
At this point we will see FSharpML comming into play enabling us to use the beloved pipelining style familiar to FSharp users.
We will now create an EstimatorChain which converts the text label to a bool then featurizes the text, and add a linear trainer.
*)

let estimatorModel = 
    EstimatorChain()
    |> Estimator.append (mlContext.Transforms.Conversion.ValueMap(["ham"; "spam"],[false; true],[| struct ("Label", "LabelText") |]))
    |> Estimator.append (mlContext.Transforms.Text.FeaturizeText("Features", "Message"))
    |> (Estimator.appendCacheCheckpoint mlContext)
    |> Estimator.append (mlContext.BinaryClassification.Trainers.StochasticDualCoordinateAscent("Label", "Features"))

(**
This is already pretty fsharp-friendly but we thought we could even closer by releaving us from carring around our instance of the MLContext
explicitly. For this we created the type EstimatorModel which contains our EstimatorChain and the context. By Calling append by we only have to provide a
lambda expression were we can define which method we want of our context.
*)

let estimatorModel' = 
    EstimatorModel.create mlContext
    |> EstimatorModel.appendBy (fun mlc -> mlc.Transforms.Conversion.ValueMap(["ham"; "spam"],[false; true],[| struct ("Label", "LabelText") |]))
    |> EstimatorModel.appendBy (fun mlc -> mlc.Transforms.Text.FeaturizeText("Features", "Message"))
    |> EstimatorModel.appendCacheCheckpoint
    |> EstimatorModel.appendBy (fun mlc -> mlc.BinaryClassification.Trainers.StochasticDualCoordinateAscent("Label", "Features"))

(**
Way better. Now we can concentrate on machine learning. So lets start by fitting our EstimatorModel to the complete data set.
*)

let model = EstimatorModel.fit data estimatorModel'
let out   = TransformerModel.transform data model

(**
Evaluation, see how good we are, may missleading because we evaluated on the training data set
*)

(**
TrainTestSplit, to get a more meaningful evaluation
*)

(**
CrossValidation, look at variance of the performance
*)

(**
back to train test split, look at precision recall curves, determine probability threshold
*)

(**
classify examples
*)

///
// Evaluate the model using cross-validation.
// Cross-validation splits our dataset into 'folds', trains a model on some folds and 
// evaluates it on the remaining fold. We are using 5 folds so we get back 5 sets of scores.
// Let's compute the average AUC, which should be between 0.5 and 1 (higher is better).
//let cvResults = mlContext.BinaryClassification.CrossValidate(data, estimatorModel.EstimatorChain |> Estimator.downcastEstimator, numFolds = 5,stratificationColumn = null)
//let avgAuc = cvResults |> Seq.map (fun struct (metrics,_,_) -> metrics.Auc) |> Seq.average
////printfn "The AUC is %f" avgAuc




//model
//|> Evaluation.BinaryClassificationEvalute() data

///// run till here, then continue, breaks 
//let x = out.Preview().ColumnView
/// run till here, works
//x |> Seq.map (fun x -> x.Column.Name)

//let downcastOtherPipeline (pipeline : ITransformer) =
//    match pipeline with
//    | :? IPredictionTransformer<IPredictor> as p -> p
//    | _ -> failwith "The pipeline has to be an instance of IEstimator<ITransformer>."

//let permutationMetrics = 
//    mlContext.BinaryClassification.PermutationFeatureImportance(model.LastTransformer |> downcastOtherPipeline,out,"Label","Features",false,permutationCount=1)

// The dataset we have is skewed, as there are many more non-spam messages than spam messages.
// While our model is relatively good at detecting the difference, this skewness leads it to always
// say the message is not spam. We deal with this by lowering the threshold of the predictor. In reality,
// it is useful to look at the precision-recall curve to identify the best possible threshold.
//let newModel = 
//    let lastTransformer = 
//        BinaryPredictionTransformer<IPredictorProducing<float32>>(
//            mlContext, 
//            model.LastTransformer.Model, 
//            model.GetOutputSchema(data.Schema), 
//            model.LastTransformer.FeatureColumn, 
//            threshold = 0.15f, 
//            thresholdColumn = DefaultColumnNames.Probability);
//    let parts = model |> Seq.toArray
//    parts.[parts.Length - 1] <- lastTransformer :> _
//    TransformerChain<ITransformer>(parts)


//// Create a PredictionFunction from our model 
//let predictor = newModel.CreatePredictionEngine<SpamInput, SpamPrediction>(mlContext);


//let classify (p : PredictionEngine<_,_>) x = 
//    let prediction = p.Predict({LabelText = ""; Message = x})
//    printfn "The message '%s' is %b" x prediction.PredictedLabel

//// Test a few examples
//[
//    "That's a great idea. It should work."
//    "free medicine winner! congratulations"
//    "Yes we should meet over the weekend!"
//    "you win pills and free entry vouchers"
//] 
//|> List.iter (classify predictor)


