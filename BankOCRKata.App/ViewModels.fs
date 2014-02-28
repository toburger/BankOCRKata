namespace Shell

type IShell = interface end

namespace ViewModels

open Shell
open Models

open System.ComponentModel.Composition
open Caliburn.Micro
open System
open System.Windows
open System.Windows.Controls
open System.Windows.Data

[<Export(typeof<IShell>)>]
type ShellViewModel [<ImportingConstructor>] (windowManager: IWindowManager) as self =
    inherit Screen(DisplayName = "Bank OCR")

    let tileSize = 200.
    let mutable previewText = null
    let mutable previewVisibility = Visibility.Collapsed
    let mutable previewPosition = Point(0., 0.)
    
    let setPosition (e: DragEventArgs) (element) =
        let pos = e.GetPosition element
        let left = pos.X - (tileSize / 2.)
        let top = pos.Y - (tileSize / 2.)
        self.PreviewPosition <- Point(left, top)

    interface IShell

    member self.TileSize = tileSize
    member val ParsedAccountNumbers: BindableCollection<Result> = BindableCollection<Result>() with get, set
    member val CachedParsing: bool = true with get, set

    member self.PreviewText
        with get() = previewText
        and set(v) =
            previewText <- v
            self.NotifyOfPropertyChange <@ self.PreviewText @>

    member self.PreviewVisibility
        with get() = previewVisibility
        and set(v) =
            previewVisibility <- v
            self.NotifyOfPropertyChange <@ self.PreviewVisibility @>

    member self.PreviewPosition
        with get() = previewPosition
        and set(v) =
            previewPosition <- v
            self.NotifyOfPropertyChange <@ self.PreviewPosition @>

    member self.OnDragEnter (e: DragEventArgs) (control: FrameworkElement) =
        if e.Data.GetDataPresent(DataFormats.FileDrop) then 
            let file = e.Data.GetData(DataFormats.FileDrop) :?> string [] |> Seq.head
            self.PreviewText <- null
            setPosition e control
            self.PreviewVisibility <- Visibility.Visible
            async {
                let preview = Utils.readPreviewOfFile 11 file
                Execute.BeginOnUIThread
                    (fun _ -> self.PreviewText <- preview)
            }
            |> Async.Start

    member self.OnDragOver (e: DragEventArgs) (control: FrameworkElement) = setPosition e control

    member self.OnDragLeave (e: RoutedEventArgs) = self.PreviewVisibility <- Visibility.Collapsed

    member self.OnDrop (e: DragEventArgs) = Async.Start <| async {
        self.PreviewVisibility <- Visibility.Collapsed

        if e.Data.GetDataPresent(DataFormats.FileDrop) then
            self.ParsedAccountNumbers.Clear()
            let files = e.Data.GetData(DataFormats.FileDrop) :?> string []
//            let! progress = self.AsyncShowProgress("just a second!", "")
            for file in files do
                try
//                    progress.SetProgress(0.5)
//                    progress.SetMessage(sprintf "I'm parsing the file: %s" file)
                    let! res =
                        AccountNumberParser.parse
                            self.CachedParsing
                            self.ParsedAccountNumbers.Add
                            file
//                    do! self.AsyncShowMessage("yeeehaw!", sprintf "%d Account numbers where parsed." res.Length)
//                    do! Async.Sleep 500
                    ()
                with _ ->
                    ()
//                    do! Async.Sleep 500
//                    do! progress.AsyncClose()
//                    do! self.AsyncShowMessage("uuups!", (sprintf "Error while parsing the file: %s." file))
//            do! progress.AsyncClose()
    }