namespace BankOCRKata

open BankOCRKata.Utils

type internal DigitString = string

and internal DigitsString = string list

type internal Digit = 
    | Digit of int
    | NonDigit of string
    override self.ToString() = 
        match self with
        | Digit d -> string d
        | NonDigit _ -> "?"

and internal Digits = Digit list

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Digit = 
    let digitsTable : DigitsString = 
        [ " _ | ||_|"; "     |  |"; " _  _||_ "; " _  _| _|"; "   |_|  |"; " _ |_  _|"; " _ |_ |_|"; " _   |  |"; 
          " _ |_||_|"; " _ |_| _|" ]
    
    let digitsTableReversed = 
        digitsTable
        |> List.mapi (fun i d -> (d : DigitString), i)
        |> dict
    
    let newDigit i = 
        if (i < 0 || i > 9) then invalidArg "i" "between 1 and 9"
        digitsTable.[i]
    
    let getDigits number = 
        [ for i = int (floor (log10 (float number))) downto 0 do
              yield int (number / pown 10 i % 10) ]
    
    let newDigits : int -> DigitsString = getDigits >> List.map newDigit
    let createDigits = newDigits >> String.concat ""
    
    let getNearest d = 
        [ 0..9 ]
        |> List.map newDigit
        |> List.mapi (fun i d' -> 
               i, 
               d
               |> Seq.zip d'
               |> Seq.map (fun (c1, c2) -> c1 = c2)
               |> Seq.filter (fun b -> b = true)
               |> Seq.sumBy (fun b -> 1))
        |> List.filter (fun (_, c) -> c = 8)
        |> List.map fst
    
    let getNearestMemoized : string -> int list = memoize getNearest
    let getNearestMemoizedOfInt : int -> int list = memoize (newDigit >> getNearest)
    
    let getDigitStrings length (s : string) = 
        [ for i in 0..s.Length / length - 1 do
              yield s.[i * length..i * length + length - 1] ]
    
    let parseDigit s = 
        match digitsTableReversed.TryGetValue s with
        | true, v -> Digit v
        | false, _ -> NonDigit s
    
    let parseDigits length s : Digits = getDigitStrings length s |> List.map parseDigit
    
    let isLegible = 
        List.forall (function 
            | Digit _ -> true
            | NonDigit _ -> false)
    
    let validChecksum ds = 
        if isLegible ds then 
            ds
            |> List.map (function 
                   | Digit d -> d
                   | NonDigit _ -> invalidOp "cannot checksum a invalid digit")
            |> (getNumber >> getDigits)
            |> checksum
        else false
    
    let getAlternativeNumber = getNearestMemoizedOfInt
    let getAlternativeNumbers = getDigits >> List.map getAlternativeNumber
    let getAlternativeNumbersAndSelf i = getDigits i |> List.map (fun i -> i :: getAlternativeNumber i)
    
    let getValidAlternativeNumbers i = 
        let ds = getDigits i
        getAlternativeNumbersAndSelf i
        |> cart
        |> Seq.filter (getDiffCount ds >> (=) 1)
        |> Seq.filter checksum
        |> Seq.map getNumber
        |> Seq.filter ((<>) i)
        |> Seq.sort
        |> Seq.toList
    
    let asNumber = 
        function 
        | Digit v -> v
        | NonDigit _ -> failwith "not a valid digit"
    
    let getAmbivalences digits = 
        digits
        |> List.map asNumber
        |> getNumber
        |> getValidAlternativeNumbers
        |> List.map (getDigits >> List.map Digit)
    
    let getAlternativesForIllegible ds = 
        ds
        |> List.map (function 
               | Digit n -> [ n ]
               | NonDigit d -> getNearestMemoized d)
        |> cart
        |> Seq.filter (fun n -> checksum n)
        |> Seq.map (List.map Digit)
        |> Seq.toList
