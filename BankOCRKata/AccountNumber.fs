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
        | Illegible ds -> sprintf "%s ILL" <| fmtDigits ds
        | Error ds -> sprintf "%s ERR" <| fmtDigits ds
    member self.Digits =
        match self with
        | Valid ds
        | Error ds
        | Illegible ds
        | Ambivalent(ds, _) -> ds
    interface System.IComparable with
        member self.CompareTo(other) =
            match other with
            | :? AccountNumber as other ->
                match self, other with
                | Valid ds1, Valid ds2
                | Error ds1, Error ds2
                | Ambivalent(ds1, _), Ambivalent(ds2, _) ->
                    let number1 = ds1 |> List.map asNumber |> getNumber
                    let number2 = ds2 |> List.map asNumber |> getNumber
                    compare number1 number2
                | self, other ->
                    compare self.Digits other.Digits
            | _ -> -1
    override self.Equals(other) =
        match other with
        | :? AccountNumber as other -> self.Digits = other.Digits
        | _ -> false
    override self.GetHashCode() =
        hash self.Digits

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
