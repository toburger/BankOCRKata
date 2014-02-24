module MainApp

open System
open System.Windows
open System.Windows.Controls
open FSharpx
open MahApps.Metro.Controls
open MahApps.Metro.Controls.Dialogs

type MainWindow = XAML< "MainWindow.xaml" >

type Result = 
    { Original : string
      Parsed : BankOCRKata.AccountNumber }

type MetroWindow with
    
    member self.AsyncShowMessage(title, message) = 
        self.ShowMessageAsync(title, message)
        |> Async.AwaitTask
        |> Async.Ignore
    
    member self.InvokeOnUI f = self.Dispatcher.BeginInvoke(Action(f)) |> ignore

let loadWindow() = 
    let window = MainWindow()

    let self = window.Root
    
    let parse onAdd file = 
        Utils.readSampleFile file
        |> Seq.map (fun (orig, s) -> 
            async { 
                let an = BankOCRKata.AccountNumber.parse s
                self.InvokeOnUI(fun _ -> onAdd ({ Original = orig; Parsed = an }))
            })
        |> Async.Parallel
        |> Async.Ignore
        |> Async.StartImmediate
        |> ignore

    self.DragEnter.Add(fun e -> e.Effects <- DragDropEffects.Copy)

    self.Drop.Add(fun e -> Async.StartImmediate <| async { 
        if e.Data.GetDataPresent(DataFormats.FileDrop) then 
            window.progress.IsActive <- true
            window.results.Items.Clear()
            let files = e.Data.GetData(DataFormats.FileDrop) :?> string []
            for file in files do
                try 
                    parse (fun res -> window.results.Items.Add(res) |> ignore) file
                with _ -> 
                    do! self.AsyncShowMessage("Error", (sprintf "Error while parsing the file: %s." file))
            window.progress.IsActive <- false
    })

    self

[<STAThread>]
(new Application()).Run(loadWindow()) |> ignore
