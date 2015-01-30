#r @"packages/FAKE/tools/FakeLib.dll"

open System
open Fake
open Fake.AssemblyInfoFile
open Fake.NuGet.Install

let version = "0.1.0.0"
let appName = "BankOCRKata.App"
let publisher = "Tobias Burger"
let cert = "./cert.pfx"

let buildDir = "./build/"
let publishDir = "./publish/"
let publishDirVersioned = sprintf "./%s/%s/" publishDir version

Target "Clean" (fun _ ->
    CleanDir buildDir)

Target "CleanPublish" (fun _ ->
    CleanDir publishDir)

let fakePrune dir =
    !! (dir + "/**/*.pdb")
    ++ (dir + "/**/*.xml")
    |> DeleteFiles

Target "Prune" (fun _ -> fakePrune buildDir)

Target "PrunePublish" (fun _ -> fakePrune publishDir)

let fakeLibrary dir =
    !! "BankOCRKata/*.fsproj"
       |> MSBuildRelease dir "Build"
       |> Log "Library Project Output: "

Target "BuildLibrary" (fun _ -> fakeLibrary buildDir)

Target "PublishLibrary" (fun _ -> fakeLibrary publishDirVersioned)

let fakeConsole dir =
    !! "BankOCRKata.Console/*.fsproj"
       |> MSBuildRelease dir "Build"
       |> Log "Console Project Output: "

Target "BuildConsole" (fun _ -> fakeConsole buildDir)

Target "PublishConsole" (fun _ -> fakeConsole publishDirVersioned)
    
let fakeWpf dir =
    !! "BankOCRKata.App/*.fsproj"
       |> MSBuildRelease dir "Build"
       |> Log "WPF Project Output: "

Target "BuildWPF" (fun _ -> fakeWpf buildDir)

Target "PublishWPF" (fun _ -> fakeWpf publishDirVersioned)

Target "BuildTests" (fun _ ->
    !! "BankOCRKata.Tests/*.fsproj"
    |> MSBuildRelease buildDir "Build"
    |> Log "Tests Project Output: ")

Target "RunTests" (fun _ ->
    NugetInstall id "xunit.runners"
    !! (buildDir + "BankOCRKata.Tests.dll")
       |> xUnit (fun p -> { p with Verbose = true
                                   WorkingDir = buildDir
                                   ShadowCopy = false
                                   XmlOutput = true
                                   OutputDir = buildDir }))

Target "PublishClickOnce" (fun _ ->
    let appManifest = sprintf "%s/%s.exe.manifest" publishDirVersioned appName
    let depManifest = sprintf "%s/%s.application" publishDir appName

    MageRun({ ApplicationFile = depManifest
              CertFile = Some(cert)
              CodeBase = None
              FromDirectory = publishDirVersioned
              IconFile = ""
              IconPath = ""
              IncludeProvider = None
              Install = Some(true)
              Manifest = appManifest
              Name = appName
              Password = None
              Processor = MageProcessor.X86
              ProjectFiles = []
              ProviderURL = ""
              Publisher = Some(publisher)
              SupportURL = None
              TmpCertFile = ""
              ToolsPath = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\" // TODO
              TrustLevel = None
              UseManifest = None
              Version = version }))

Target "Zip" (fun _ ->
    !! (publishDirVersioned + "/**/*.*")
       -- "/**/*.zip"
       -- "/**/*.xml"
       -- "/**/*.pdb"
       |> Zip publishDirVersioned (sprintf "%s/BankOCRKata.%s.zip" publishDir version))

Target "Default" id

"Clean"
==> "BuildLibrary"

// Build the Console Project
"BuildLibrary"
==> "BuildConsole"

// Build the WPF Project
"BuildLibrary"
==> "BuildWPF"

// WPF is the default
"BuildWPF"
==> "Default"

// Build the Tests Project
"BuildLibrary"
==> "BuildTests"

// Run Unit Tests
"BuildTests"
==> "RunTests"

"CleanPublish"
==> "PublishLibrary"

"PublishLibrary"
==> "PublishConsole"

"PublishLibrary"
==> "PublishWPF"

// Publish as Click once application
"PublishWPF"
==> "PrunePublish"
==> "PublishClickOnce"

// Create a ZIP file
"PublishWPF"
==> "Zip"

RunTargetOrDefault "Default"
