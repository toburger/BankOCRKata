﻿open BankOCRKata
open System.IO

let readSampleFile filename = seq {
    let lines = File.ReadLines filename

    let digitLineSize = 4
        
    let getDigitChunks (s : string) = 
        [ for i in [ 0..3..78 ] -> s.[i..i + 2] ]
        
    let getDigits (xs : string list) = 
        [ for i in [ 0..9 - 1 ] -> xs.[i] + xs.[i + 9] + xs.[i + 18] ]
        
    yield! lines
    |> Seq.chunk digitLineSize
    |> Seq.map (fun window ->
        window.[0] + window.[1] + window.[2]
        |> getDigitChunks
        |> getDigits
        |> String.concat ""
    )
}

[<EntryPoint>]
let main argv = 

    // parse number after number
    //readSampleFile "sample.txt"
    //|> Seq.map AccountNumber.parse
    //|> Seq.iter (printfn "%O")

    // parse in parallel
    readSampleFile "sample.txt"
    |> Seq.map (fun s -> async { return AccountNumber.parse s })
    |> Async.Parallel
    |> Async.RunSynchronously
    |> Seq.iter (printfn "%O")

    0
