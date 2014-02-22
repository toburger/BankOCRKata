module BankOCRKata.Tests

open BankOCRKata.Types
open BankOCRKata.Digit
open BankOCRKata.AccountNumber
open Xunit
open Xunit.Extensions
open FsUnit.Xunit

[<Theory>]
[<InlineData(0, " _ | ||_|")>]
[<InlineData(1, "     |  |")>]
[<InlineData(2, " _  _||_ ")>]
[<InlineData(3, " _  _| _|")>]
[<InlineData(6, " _ |_ |_|")>]
[<InlineData(9, " _ |_| _|")>]
let ``Create Digit from number`` (input, expected) =
    newDigit input
    |> should equal expected

[<Fact>]
let ``Create Digits from number`` () =
    newDigits 12 |> should equal [ "     |  |"; " _  _||_ " ]
    newDigits 94 |> should equal [ " _ |_| _|"; "   |_|  |" ]

[<Theory>]
[<InlineData(457508000, "457508000")>]
[<InlineData(298749824, "290749824")>]
[<InlineData(131231212, "ERR 131231212")>]
let ``Parse legible AccountNumbers`` (input, expected) =
    parse (createDigits input)
    |> should equal expected

[<Theory>]
[<InlineData(111111111, "711111111")>]
[<InlineData(777777777, "777777177")>]
[<InlineData(200000000, "200800000")>]
[<InlineData(333333333, "333393333")>]
[<InlineData(888888888, "888888888 AMB ['888886888', '888888880', '888888988']")>]
[<InlineData(555555555, "555555555 AMB ['555655555', '559555555']")>]
[<InlineData(666666666, "666666666 AMB ['666566666', '686666666']")>]
[<InlineData(999999999, "999999999 AMB ['899999999', '993999999', '999959999']")>]
[<InlineData(490067715, "490067715 AMB ['490067115', '490067719', '490867715']")>]
let ``Parse legible AccountNumbers from Web Page sample`` (input, expected) =
    parse (createDigits input)
    |> should equal expected

[<Fact>]
let ``Parse illegible AccountNumbers from Web Page sample`` () =
    let inputs = [
        "    _|  |" + createDigits 23456789,                 "123456789"
        createDigits 0 + "   | ||_|" + createDigits 0000051, "000000051" // not correctly created, but in this case it makes no difference
        createDigits 49086771 + " _  _  _|",                 "490867715"
    ]

    for (input, expected) in inputs do
        parse input
        |> should equal expected

(*
    Data from OCR Kata web page (http://codingdojo.org/cgi-bin/index.pl?KataBankOCR): 

    use case 4
                           
      |  |  |  |  |  |  |  |  |
      |  |  |  |  |  |  |  |  |
                           
    => 711111111
     _  _  _  _  _  _  _  _  _ 
      |  |  |  |  |  |  |  |  |
      |  |  |  |  |  |  |  |  |
                           
    => 777777177
     _  _  _  _  _  _  _  _  _ 
     _|| || || || || || || || |
    |_ |_||_||_||_||_||_||_||_|
                           
    => 200800000
     _  _  _  _  _  _  _  _  _ 
     _| _| _| _| _| _| _| _| _|
     _| _| _| _| _| _| _| _| _|
                           
    => 333393333 
     _  _  _  _  _  _  _  _  _ 
    |_||_||_||_||_||_||_||_||_|
    |_||_||_||_||_||_||_||_||_|
                           
    => 888888888 AMB ['888886888', '888888880', '888888988']
     _  _  _  _  _  _  _  _  _ 
    |_ |_ |_ |_ |_ |_ |_ |_ |_ 
     _| _| _| _| _| _| _| _| _|
                           
    => 555555555 AMB ['555655555', '559555555']
     _  _  _  _  _  _  _  _  _ 
    |_ |_ |_ |_ |_ |_ |_ |_ |_ 
    |_||_||_||_||_||_||_||_||_|
                           
    => 666666666 AMB ['666566666', '686666666']
     _  _  _  _  _  _  _  _  _ 
    |_||_||_||_||_||_||_||_||_|
     _| _| _| _| _| _| _| _| _|
                           
    => 999999999 AMB ['899999999', '993999999', '999959999']
        _  _  _  _  _  _     _ 
    |_||_|| || ||_   |  |  ||_ 
      | _||_||_||_|  |  |  | _|
                           
    => 490067715 AMB ['490067115', '490067719', '490867715']
        _  _     _  _  _  _  _ 
     _| _| _||_||_ |_   ||_||_|
      ||_  _|  | _||_|  ||_| _| 
                           
    => 123456789
     _     _  _  _  _  _  _    
    | || || || || || || ||_   |
    |_||_||_||_||_||_||_| _|  |
                           
    => 000000051
        _  _  _  _  _  _     _ 
    |_||_|| ||_||_   |  |  | _ 
      | _||_||_||_|  |  |  | _|
                           
    => 490867715 
*)