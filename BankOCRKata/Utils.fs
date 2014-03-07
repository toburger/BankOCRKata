module internal BankOCRKata.Utils

[<Measure>] type d

let fillWithNulls length s = 
    System.String(sprintf "%*s" length s
                  |> Seq.map (function 
                         | ' ' -> '0'
                         | c -> c)
                  |> Seq.toArray)

let memoize onAdd = 
    let dict = System.Collections.Concurrent.ConcurrentDictionary<'key, 'value>(HashIdentity.Structural)
    fun x -> dict.GetOrAdd(x, System.Func<_, _>(onAdd))
    
let getDigits number = 
    [ for i = int (floor (log10 (float number))) downto 0 do
            yield int (number / pown 10 i % 10) * 1<d> ]

let checksum (is: int<d> list) = 
    let csum = 
        is
        |> List.rev
        |> List.mapi (fun i d -> (i + 1) * d)
        |> List.sum
    (csum / 1<d>) % 11 = 0

let getNumber (ns: int<d> list) = List.foldBack (fun d (i, s) -> i + 1, s + pown 10 i * (d / 1<d>)) ns (0, 0) |> snd

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
    //cart1
    cart2
    //cart3 >> Seq.map Seq.toList
    //cart4 >> Seq.map Seq.toList

let flip f x y = f y x

let getDiffCount xs ys =
    List.fold2 (fun state x y ->
        if x <> y
        then state + 1
        else state
    ) 0 xs ys
