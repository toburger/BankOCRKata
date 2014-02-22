module BankOCRKata.Tests

open BankOCRKata.Digit
open BankOCRKata.AccountNumber
open Xunit
open Xunit.Extensions
open FsUnit.Xunit

module UtilTests =
    [<Theory>]
    [<InlineData("123", 8, "00000123")>]
    [<InlineData("",    8, "00000000")>]
    [<InlineData("123", 1, "123")>]
    let ``Test Utils.fillWithNulls`` (input, length, expected) =
        input
        |> Utils.fillWithNulls length
        |> should equal expected

//    [<Theory>]
//    [<InlineData(12345, [|1;2;3;4;5|])>]
//    [<InlineData(0123,  [|1;2;3|])>]
//    let ``Test Utils.getDigits`` (input: int, expected: int list) =
//        input
//        |> Utils.getDigits
//        |> should equal expected
//
//    [<Theory>]
//    [<InlineData([|1;2;3;4;5|], 12345)>]
//    [<InlineData([|0;1;2;3|],   123)>]
//    let ``Test Utisl.getNumber`` (input: int list, expected: int) =
//        input
//        |> Utils.getNumber
//        |> should equal expected

    [<Fact>]
    let ``Test Utils.cart`` () =
        [[1;2];[3]]
        |> Utils.cart
        |> List.ofSeq
        |> should equal [[1;3];[2;3]]

    [<Fact>]
    let ``Test Utils.getDiffCount`` () =
        [1;2;3;4]
        |> Utils.getDiffCount [1;2;4;4]
        |> should equal 1

    [<Theory>]
    [<InlineData(456785876, true)>]
    [<InlineData(543214567, true)>]
    [<InlineData(987654321, false)>]
    let ``Test Utils.checksum`` (input, expected) =
        input
        |> Utils.getDigits
        |> Utils.checksum
        |> should equal expected

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
    parseToString (createDigits input)
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
    parseToString (createDigits input)
    |> should equal expected

[<Fact>]
let ``Parse illegible AccountNumbers from Web Page sample`` () =
    let inputs = [
        "    _|  |" + createDigits 23456789,                 "123456789"
        newDigit 0 + "   | ||_|" + newDigit 0 + newDigit 0 + newDigit 0 + newDigit 0 + newDigit 0 + createDigits 51, "000000051"
        createDigits 49086771 + " _  _  _|",                 "490867715"
    ]

    for (input, expected) in inputs do
        parseToString input
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