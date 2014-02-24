module Utils

open System.IO
open Microsoft.FSharp.Control

module Seq = 
    let chunk n xs = 
        seq { 
            let i = ref 0
            let arr = ref <| Array.create n (Unchecked.defaultof<'a>)
            for x in xs do
                if !i = n then 
                    yield !arr
                    arr := Array.create n (Unchecked.defaultof<'a>)
                    i := 0
                (!arr).[!i] <- x
                i := !i + 1
            if !i <> 0 then yield (!arr).[0..!i - 1]
        }

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
        String.concat "\n" window.[0..2],
        window.[0] + window.[1] + window.[2]
        |> getDigitChunks
        |> getDigits
        |> String.concat ""
    )
}

let readPreviewOfFile lineCount file =
    System.IO.File.ReadLines(file)
    |> Seq.truncate lineCount
    |> String.concat "\n"

let memoize onAdd = 
    let dict = System.Collections.Concurrent.ConcurrentDictionary<'key, 'value>(HashIdentity.Structural)
    fun x -> dict.GetOrAdd(x, System.Func<_, _>(onAdd))

let (|Value|Null|) (nullable: System.Nullable<_>) =
    if nullable.HasValue
    then Value (nullable.Value)
    else Null