module MainApp

open System
open System.Collections.ObjectModel
open System.ComponentModel
open System.Windows
open System.Windows.Controls
open System.Windows.Data
open FSharpx
open MahApps.Metro.Controls
open MahApps.Metro.Controls.Dialogs

type MainWindow = XAML< "MainWindow.xaml" >

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

let mutable cache: bool = true

let loadWindow() = 
    let window = MainWindow()

    let self = window.Root

    window.cache.IsChecked <- Nullable(cache)

    let collection = ObservableCollection()
    ignore <| window.results.SetBinding(ListView.ItemsSourceProperty, Binding(Source = collection, IsAsync = true))
    let view = CollectionViewSource.GetDefaultView(window.results.ItemsSource) :?> CollectionView
    view.SortDescriptions.Add(SortDescription())
    
    let parse onAdd file =

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

    let setPosition (e: DragEventArgs) =
        let pos = e.GetPosition(window.previewCanvas)
        Canvas.SetLeft(window.preview, pos.X - (window.preview.Width / 2.))
        Canvas.SetTop(window.preview, pos.Y - (window.preview.Height / 2.))

    self.DragEnter.Add(fun e ->
        if e.Data.GetDataPresent(DataFormats.FileDrop) then 
            let file = e.Data.GetData(DataFormats.FileDrop) :?> string [] |> Seq.head
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
            collection.Clear()
            let files = e.Data.GetData(DataFormats.FileDrop) :?> string []
//            let! progress = self.AsyncShowProgress("just a second!", "")
            for file in files do
                try
//                    progress.SetProgress(0.5)
//                    progress.SetMessage(sprintf "I'm parsing the file: %s" file)
                    let! res = parse (fun res -> self.InvokeOnUI(fun _ -> collection.Add res)) file
                    do! self.AsyncShowMessage("yeeehaw!", sprintf "%d Account numbers where parsed." res.Length)
//                    do! Async.Sleep 500
                with _ ->
//                    do! Async.Sleep 500
//                    do! progress.AsyncClose()
                    do! self.AsyncShowMessage("uuups!", (sprintf "Error while parsing the file: %s." file))
//            do! progress.AsyncClose()
    })

    self.KeyDown.Add(fun e ->
        match e.Key with
        | Input.Key.Escape -> Async.CancelDefaultToken()
        | _ -> ())

    let checkedOrUnchecked _ =
        cache <-
            match window.cache.IsChecked with
            | Utils.Null -> false
            | Utils.Value v -> v

    window.cache.Checked.Add(checkedOrUnchecked)

    window.cache.Unchecked.Add(checkedOrUnchecked)

    self

[<STAThread>]
(new Application()).Run(loadWindow()) |> ignore
