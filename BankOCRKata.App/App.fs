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

    member self.AsyncShowProgress(title, message) =
        self.ShowProgressAsync(title, message)
        |> Async.AwaitTask
    
    member self.InvokeOnUI f = self.Dispatcher.BeginInvoke(Action(f)) |> ignore

type ProgressDialogController with

    member self.AsyncClose() =
        if self.IsOpen
        then self.CloseAsync() |> Async.AwaitIAsyncResult |> Async.Ignore
        else async.Return()

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

    let setPosition (e: DragEventArgs) =
        let pos = e.GetPosition(window.previewCanvas)
        Canvas.SetLeft(window.preview, pos.X - (window.preview.Width / 2.))
        Canvas.SetTop(window.preview, pos.Y - (window.preview.Height / 2.))

    self.DragEnter.Add(fun e ->
        if e.Data.GetDataPresent(DataFormats.FileDrop) then 
            let file = (e.Data.GetData(DataFormats.FileDrop) :?> string []).[0]
            window.preview.Content <- null
            setPosition e
            window.preview.Visibility <- Visibility.Visible
            async { 
                let preview = Utils.readPreviewOfFile 11 file
                self.InvokeOnUI
                    (fun _ -> window.preview.Content <- TextBlock(Text = preview,
                                                                  Foreground = window.preview.Foreground,
                                                                  TextWrapping = TextWrapping.NoWrap,
                                                                  FontFamily = Media.FontFamily("Consolas")))
            }
            |> Async.Start)

    self.DragOver.Add(fun e -> setPosition e)

    self.DragLeave.Add(fun e -> window.preview.Visibility <- Visibility.Collapsed)

    self.Drop.Add(fun e -> Async.StartImmediate <| async {
        window.preview.Visibility <- Visibility.Collapsed

        if e.Data.GetDataPresent(DataFormats.FileDrop) then
            window.results.Items.Clear()
            let files = e.Data.GetData(DataFormats.FileDrop) :?> string []
            let! progress = self.AsyncShowProgress("just a second!", "")
            for file in files do
                try 
                    progress.SetProgress(0.5)
                    progress.SetMessage(sprintf "I'm parsing the file: %s" file)
                    do! parse (fun res -> window.results.Items.Add(res) |> ignore) file
                    do! Async.Sleep 500
                with _ ->
                    do! Async.Sleep 500
                    do! progress.AsyncClose()
                    do! self.AsyncShowMessage("uuups!", (sprintf "Error while parsing the file: %s." file))
            do! progress.AsyncClose()
    })

    self

[<STAThread>]
(new Application()).Run(loadWindow()) |> ignore
