module MainApp

open System
open System.Windows
open System.Windows.Controls
open FSharpx

type MainWindow = XAML< "MainWindow.xaml" >

type Result = 
    { Original : string
      Parsed : BankOCRKata.AccountNumber }

let loadWindow() = 
    let window = MainWindow()
    let invokeOnUI f = window.Root.Dispatcher.BeginInvoke(Action(f)) |> ignore
    window.parse.Click.Add(fun _ -> 
        window.results.Items.Clear()
        Utils.readSampleFile "sample.txt"
//        |> Seq.iter (fun (orig, s) ->
//            let an = BankOCRKata.AccountNumber.parse s
//            let res = { Original = orig; Parsed = an }
//            window.results.Items.Add res |> ignore
//        ))
        |> Seq.map (fun res -> 
            let (orig, s) = res
            async {
                let an = BankOCRKata.AccountNumber.parse s
                let res = { Original = orig; Parsed = an }
                invokeOnUI (fun _ -> window.results.Items.Add(res) |> ignore)
            })
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore)
    window.Root

[<STAThread>]
(new Application()).Run(loadWindow()) |> ignore
