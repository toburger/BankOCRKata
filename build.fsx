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

Target "RunTests" (fun _ ->
    !! (buildDir + "xunit.dll")
       |> xUnit (fun p -> { p with Verbose = true
                                   WorkingDir = buildDir
                                   ShadowCopy = false
                                   XmlOutput = true
                                   OutputDir = buildDir }))

Target "Zip" (fun _ ->
    !! (buildDir + "/**/*.*")
       -- "/**/*.zip"
       -- "/**/*.xml"
       -- "/**/*.pdb"
       |> Zip buildDir (buildDir + "BankOCRKata.zip"))

Target "Default" id

// Build the Console Project
"Clean"
==> "BuildLibrary"
==> "BuildConsole"

// Build the WPF Project
"Clean"
==> "BuildLibrary"
==> "BuildWPF"

// Run the unit tests
"Clean"
==> "BuildLibrary"
==> "RunTests"

// WPF is the default
"BuildWPF"
==> "Default"

// Create a ZIP file
"Default"
==> "Zip"

RunTargetOrDefault "Default"
