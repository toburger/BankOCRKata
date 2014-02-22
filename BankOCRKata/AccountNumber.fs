namespace BankOCRKata

open BankOCRKata.Utils
open BankOCRKata.Digit

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
    
    let parse s = 
        match parseDigits s with
        | Legible & Valid & digits -> Valid digits
        | Legible & Invalid & Ambivalences ambs & digits -> Ambivalent(digits, ambs)
        | Legible & Invalid & SingleAmbivalence amb -> Valid amb
        | Legible & Invalid & NoAmbivalences & digits -> Error digits
        | Illegible & SingleAlternative alt -> Valid alt
        | Illegible & Alternatives alts & digits -> Ambivalent(digits, alts)
        | Illegible & NoAlternatives & digits -> Illegible digits
    
    let parseToString = parse >> sprintf "%O"
