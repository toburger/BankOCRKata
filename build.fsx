// --------------------------------------------------------------------------------------
// FAKE build script 
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"
open System
open Fake

RestorePackages()

let buildDir = "./build/"

Target "Clean" (fun _ ->
    CleanDir buildDir)

Target "BuildLibrary" (fun _ ->
    !! "BankOCRKata/*.fsproj"
    |> MSBuildRelease buildDir "Build"
    |> Log "Library Project Output: ")

Target "BuildConsole" (fun _ ->
    !! "BankOCRKata.Console/*.fsproj"
    |> MSBuildRelease buildDir "Build"
    |> Log "Console Project Output: ")

Target "BuildWPF" (fun _ ->
    !! "BankOCRKata.App/*.fsproj"
    |> MSBuildRelease buildDir "Build"
    |> Log "WPF Project Output: ")

Target "Default" (fun _ ->
    trace "Hello World from FAKE")

// Build the Console Project
"Clean"
==> "BuildLibrary"
==> "BuildConsole"

// Build the WPF Project
"Clean"
==> "BuildLibrary"
==> "BuildWPF"

// WPF is the default
"BuildWPF"
==> "Default"

RunTargetOrDefault "Default"
