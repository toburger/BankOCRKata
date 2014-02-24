﻿module MainApp

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

    let parse onAdd file =
        Utils.readSampleFile file
        |> Seq.map (fun (orig, s) ->
            async {
                let an = BankOCRKata.AccountNumber.parse s
                invokeOnUI (fun _ -> onAdd({ Original = orig; Parsed = an }))
            })
        |> Async.Parallel
        |> Async.Ignore
        |> Async.StartImmediate
        |> ignore

    window.results.DragEnter.Add(fun e -> e.Effects <- DragDropEffects.Copy)

    window.results.Drop.Add(fun e ->
        if e.Data.GetDataPresent(DataFormats.FileDrop) then
            window.progress.IsActive <- true
            window.results.Items.Clear()
            let files = e.Data.GetData(DataFormats.FileDrop) :?> string[]
            for file in files do
                try
                    parse (fun res -> window.results.Items.Add(res) |> ignore) file
                with _ ->
                    MessageBox.Show(window.Root, "Error while parsing the file.") |> ignore
            window.progress.IsActive <- false
    )

    window.Root

[<STAThread>]
(new Application()).Run(loadWindow()) |> ignore
