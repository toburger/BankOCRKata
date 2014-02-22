module internal BankOCRKata.Utils

let fillWithNulls length s = 
    System.String(sprintf "%*s" length s
                  |> Seq.map (function 
                         | ' ' -> '0'
                         | c -> c)
                  |> Seq.toArray)

let inline memoize onAdd onGet = 
    let dict = System.Collections.Concurrent.ConcurrentDictionary<_, _>(HashIdentity.Structural)
    fun x -> 
        if dict.ContainsKey x then onGet dict.[x]
        else 
            let v = onAdd x
            dict.TryAdd(x, v) |> ignore
            v

let checksum is = 
    let csum = 
        is
        |> List.rev
        |> List.mapi (fun i d -> (i + 1) * d)
        |> List.sum
    csum % 11 = 0

let getNumber ns = List.foldBack (fun d (i, s) -> i + 1, s + pown 10 i * d) ns (0, 0) |> snd

let rec cart1 nll = 
    let f0 n = 
        function 
        | [] -> [ [ n ] ]
        | nll -> List.map (fun nl -> n :: nl) nll
    match nll with
    | [] -> []
    | h :: t -> List.collect (fun n -> f0 n (cart1 t)) h

let rec cart2 = 
    function 
    | [] -> Seq.singleton []
    | L :: Ls -> L |> Seq.collect (fun x -> cart2 Ls |> Seq.map (fun L -> x :: L))

let cart3 sequences = 
    let step acc sequence = 
        seq { 
            for x in acc do
                for y in sequence do
                    yield seq { 
                              yield! x
                              yield y
                          }
        }
    Seq.fold step (Seq.singleton Seq.empty) sequences

let cart4 sequences = 
    let step acc sequence = 
        seq { 
            for x in acc do
                for y in sequence do
                    yield Seq.append x [ y ]
        }
    Seq.fold step (Seq.singleton Seq.empty) sequences

let cart = 
    //    cart1
    cart2
//    cart3 >> Seq.map Seq.toList
//    cart4 >> Seq.map Seq.toList
let flip f x y = f y x

let getDiffCount xs ys = 
    xs
    |> List.zip ys
    |> List.filter (fun (x, y) -> x <> y)
    |> List.length
