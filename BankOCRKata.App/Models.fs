namespace Models

open System

[<CustomComparison; CustomEquality>]
type Result =
    { Original : string
      Parsed : BankOCRKata.AccountNumber }
    interface IComparable with
        member self.CompareTo(other) =
            let res =
                match other with
                | :? Result as other -> compare self.Parsed other.Parsed
                | _ -> -1
            res
    override self.Equals(other) =
        let res =
            match other with
            | :? Result as other -> self.Original = other.Original && self.Parsed = other.Parsed
            | _ -> false
        res
    override self.GetHashCode() = hash self.Original ^^^ hash self.Parsed

module AccountNumberParser =
    let parse cache onAdd file =
    
        let parse = BankOCRKata.AccountNumber.parse
        let parsememoized = Utils.memoize parse
    
        Utils.readSampleFile file
        |> Seq.map (fun (orig, s) -> 
            async {
                let an =
                    if cache
                    then parsememoized s
                    else parse s
                onAdd { Original = orig; Parsed = an }
            })
        |> Async.Parallel
        |> Async.Ignore