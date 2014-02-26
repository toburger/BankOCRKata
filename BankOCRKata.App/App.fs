module MainApp

open System
open System.Windows

type App() =
    inherit Application()
    do Shell.AppBootstrapper() |> ignore

[<STAThread>]
(new App()).Run() |> ignore
