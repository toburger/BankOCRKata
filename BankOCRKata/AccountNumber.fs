module BankOCRKata.AccountNumber

open BankOCRKata.Types
open BankOCRKata.Digit

let (|Valid|Invalid|) digits = 
    if validChecksum digits then Valid
    else Invalid

let (|Legible|Illegible|) digits = 
    if isLegible digits then Legible
    else Illegible

let (|NoAmbivalences|SingleAmbivalence|Ambivalences|) digits = 
    match getAmbivalences digits with
    | [] -> NoAmbivalences
    | amb :: [] -> SingleAmbivalence amb
    | ambs -> Ambivalences(ambs)

let (|NoAlternatives|SingleAlternative|Alternatives|) digits = 
    match getAlternativesForIllegible digits with
    | [] -> NoAlternatives
    | alt :: [] -> SingleAlternative alt
    | alts -> Alternatives alts

let parseAccount s = 
    match parseDigits s with
    | Legible & Valid & digits -> Valid digits
    | Legible & Invalid & Ambivalences ambs & digits -> Ambivalent(digits, ambs)
    | Legible & Invalid & SingleAmbivalence amb -> Valid amb
    | Legible & Invalid & NoAmbivalences & digits -> Error digits
    | Illegible & SingleAlternative alt -> Valid alt
    | Illegible & Alternatives alts & digits -> Ambivalent(digits, alts)
    | Illegible & NoAlternatives & digits -> Illegible digits

let parse = parseAccount >> sprintf "%O"
