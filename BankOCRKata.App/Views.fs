namespace Views

open FSharpx
open System.ComponentModel
open System.Windows
open System.Windows.Controls
open System.Windows.Data

type ShellViewXAML = XAML<"ShellView.xaml">

type ShellView() as self =
    inherit UserControl()

    let xaml = ShellViewXAML()

    do self.Loaded.Add <| fun _ ->
        let view = CollectionViewSource.GetDefaultView(xaml.results.ItemsSource) :?> CollectionView
        do view.SortDescriptions.Add(SortDescription())

    do self.KeyDown.Add(fun e ->
        match e.Key with
        | Input.Key.Escape -> Async.CancelDefaultToken()
        | _ -> ())

    do self.Content <- xaml.Root
