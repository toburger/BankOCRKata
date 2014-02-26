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
type ShellViewModel [<ImportingConstructor>] (windowManager: IWindowManager) =
    inherit Screen()
    
    let setPosition e = ()

    interface IShell

    member val PreviewText: string = null with get, set
    member val PreviewVisibility: Visibility = Visibility.Collapsed with get, set
    member val PreviewPosition: Point = Point(0.0, 0.0) with get, set
    member val ParsedAccountNumbers: BindableCollection<Result> = BindableCollection<Result>() with get, set
    member val CachedParsing: bool = true with get, set

    member self.OnDragEnter (e: DragEventArgs) =
        if e.Data.GetDataPresent(DataFormats.FileDrop) then 
            let file = e.Data.GetData(DataFormats.FileDrop) :?> string [] |> Seq.head
            self.PreviewText <- null
            setPosition e
            self.PreviewVisibility <- Visibility.Visible
            async {
                let preview = Utils.readPreviewOfFile 11 file
                Execute.BeginOnUIThread
                    (fun _ -> self.PreviewText <- preview)
            }
            |> Async.Start

    member self.OnDragOver (e: DragEventArgs) = setPosition e

    member self.OnDragLeave (e: RoutedEventArgs) = self.PreviewVisibility <- Visibility.Collapsed

    member self.OnDrop (e: DragEventArgs) = Async.StartImmediate <| async {
        self.PreviewVisibility <- Visibility.Collapsed

        if e.Data.GetDataPresent(DataFormats.FileDrop) then
            self.ParsedAccountNumbers.Clear()
            let files = e.Data.GetData(DataFormats.FileDrop) :?> string []
//            let! progress = self.AsyncShowProgress("just a second!", "")
            for file in files do
                try
//                    progress.SetProgress(0.5)
//                    progress.SetMessage(sprintf "I'm parsing the file: %s" file)
                    let! res = AccountNumberParser.parse self.CachedParsing (fun res -> Execute.BeginOnUIThread(fun _ -> self.ParsedAccountNumbers.Add res)) file
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