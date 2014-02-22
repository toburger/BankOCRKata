namespace BankOCRKata.Types

open BankOCRKata.Utils

type DigitString = string

and DigitsString = string list

type Digit = Digit of int | NonDigit of string with
    override self.ToString() =
        match self with
        | Digit d -> string d
        | NonDigit _ -> "?"

and Digits = Digit list

and Account = 
    | Valid of Digits
    | Ambivalent of Digits * Digits list
    | Illegible of Digits
    | Error of Digits
    override self.ToString() = 
        let fmtDigits ds = ds |> List.map string |> String.concat "" |> fillWithNulls 9
        match self with
        | Valid ds -> sprintf "%s" <| fmtDigits ds
        | Ambivalent(ds, dss) -> 
            sprintf "%O AMB [%s]" (fmtDigits ds) (dss
                                                  |> List.map (fmtDigits >> sprintf "'%s'")
                                                  |> String.concat ", ")
        | Illegible ds -> sprintf "ILL %s" <| fmtDigits ds
        | Error ds -> sprintf "ERR %s" <| fmtDigits ds
