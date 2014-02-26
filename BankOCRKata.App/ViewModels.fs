namespace Shell

type IShell = interface end

namespace ViewModels

open System.ComponentModel.Composition
open Caliburn.Micro
open Shell

[<Export(typeof<IShell>)>]
type ShellViewModel [<ImportingConstructor>] (windowManager: IWindowManager) =
    inherit Screen()
    interface IShell