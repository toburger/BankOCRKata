namespace BankOCRKata

open BankOCRKata.Utils
open BankOCRKata.Digit

[<CustomComparison; CustomEquality>]
type AccountNumber = 
    | Valid of Digits
    | Ambivalent of Digits * Digits list
    | Illegible of Digits
    | Error of Digits
    override self.ToString() = 
        let fmtDigits ds = 
            ds
            |> List.map string
            |> String.concat ""
            |> fillWithNulls 9
        match self with
        | Valid ds -> sprintf "%s" <| fmtDigits ds
        | Ambivalent(ds, dss) -> 
            sprintf "%O AMB [%s]" (fmtDigits ds) (dss
                                                  |> List.map (fmtDigits >> sprintf "'%s'")
                                                  |> String.concat ", ")
        | Illegible ds -> sprintf "ILL %s" <| fmtDigits ds
        | Error ds -> sprintf "ERR %s" <| fmtDigits ds
    interface System.IComparable with
        member self.CompareTo(other) =
            match other with
            | :? AccountNumber as other ->
                match self, other with
                | Valid ds1, Valid ds2
                | Error ds1, Error ds2 ->
                    let number1 = ds1 |> List.map asNumber |> getNumber
                    let number2 = ds2 |> List.map asNumber |> getNumber
                    compare number1 number2
                | Illegible ds1, Illegible ds2 -> compare ds1 ds2
                | Ambivalent(ds1, dss1), Ambivalent(ds2, dss2) -> compare (ds1, dss1) (ds2, dss2)
                | Valid _, Illegible _ -> -1
                | Illegible _, Valid _ -> 0
                | Illegible _, Error _ -> -1
                | Error _, Illegible _ -> 0
                | Error _, Ambivalent _ -> -1
                | Ambivalent _, Error _ -> 0
                | _, _ -> -1
            | _ -> -1
    override self.Equals(other) =
        match other with
        | :? AccountNumber as other -> false
        | _ -> false
    override self.GetHashCode() =
        match self with
        | Valid ds
        | Illegible ds
        | Error ds -> hash ds
        | Ambivalent(ds, dss) -> hash ds ^^^ hash dss

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module AccountNumber = 
    let private (|Valid|Invalid|) digits = 
        if validChecksum digits then Valid
        else Invalid
    
    let private (|Legible|Illegible|) digits = 
        if isLegible digits then Legible
        else Illegible
    
    let private (|NoAmbivalences|SingleAmbivalence|Ambivalences|) digits = 
        match getAmbivalences digits with
        | [] -> NoAmbivalences
        | amb :: [] -> SingleAmbivalence amb
        | ambs -> Ambivalences(ambs)
    
    let private (|NoAlternatives|SingleAlternative|Alternatives|) digits = 
        match getAlternativesForIllegible digits with
        | [] -> NoAlternatives
        | alt :: [] -> SingleAlternative alt
        | alts -> Alternatives alts
    
    [<CompiledName("Parse")>]
    let parse input = 
        match parseDigits 9 input with
        | Legible & Valid & digits -> Valid digits
        | Legible & Invalid & Ambivalences ambs & digits -> Ambivalent(digits, ambs)
        | Legible & Invalid & SingleAmbivalence amb -> Valid amb
        | Legible & Invalid & NoAmbivalences & digits -> Error digits
        | Illegible & SingleAlternative alt -> Valid alt
        | Illegible & Alternatives alts & digits -> Ambivalent(digits, alts)
        | Illegible & NoAlternatives & digits -> Illegible digits
    
    let parseToString = parse >> sprintf "%O"
