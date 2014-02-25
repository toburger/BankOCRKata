#r @"packages/FAKE/tools/FakeLib.dll"

open System
open Fake
open Fake.AssemblyInfoFile

RestorePackages()

let version = "0.1.0.0"
let appName = "BankOCRKata.App"
let publisher = "Tobias Burger"
let cert = "./cert.pfx"

let buildDir = "./build/"
let deployDir = "./publish/"
let deployDirVersioned = sprintf "./%s/%s/" deployDir version

Target "Clean" (fun _ ->
    CleanDir buildDir)

Target "CleanDeploy" (fun _ ->
    CleanDir deployDir)

let fakePrune dir =
    !! (dir + "/**/*.pdb")
    ++ (dir + "/**/*.xml")
    |> DeleteFiles

Target "Prune" (fun _ -> fakePrune buildDir)

Target "PruneDeploy" (fun _ -> fakePrune deployDir)

let fakeLibrary dir =
    !! "BankOCRKata/*.fsproj"
       |> MSBuildRelease dir "Build"
       |> Log "Library Project Output: "

Target "BuildLibrary" (fun _ -> fakeLibrary buildDir)

Target "DeployLibrary" (fun _ -> fakeLibrary deployDirVersioned)

let fakeConsole dir =
    !! "BankOCRKata.Console/*.fsproj"
       |> MSBuildRelease dir "Build"
       |> Log "Console Project Output: "

Target "BuildConsole" (fun _ -> fakeConsole buildDir)

Target "DeployConsole" (fun _ -> fakeConsole deployDirVersioned)
    
let fakeWpf dir =
    !! "BankOCRKata.App/*.fsproj"
       |> MSBuildRelease dir "Build"
       |> Log "WPF Project Output: "

Target "BuildWPF" (fun _ -> fakeWpf buildDir)

Target "DeployWPF" (fun _ -> fakeWpf deployDirVersioned)

Target "BuildTests" (fun _ ->
    !! "BankOCRKata.Tests/*.fsproj"
    |> MSBuildRelease buildDir "Build"
    |> Log "Tests Project Output: ")

Target "RunTests" (fun _ ->
    !! (buildDir + "BankOCRKata.Tests.dll")
       |> xUnit (fun p -> { p with Verbose = true
                                   WorkingDir = buildDir
                                   ShadowCopy = false
                                   XmlOutput = true
                                   OutputDir = buildDir }))

Target "DeployClickOnce" (fun _ ->
    let appManifest = sprintf "%s/%s.exe.manifest" deployDirVersioned appName
    let depManifest = sprintf "%s/%s.application" deployDir appName

    MageRun({ ApplicationFile = depManifest
              CertFile = Some(cert)
              CodeBase = None
              FromDirectory = deployDirVersioned
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
    !! (deployDirVersioned + "/**/*.*")
       -- "/**/*.zip"
       -- "/**/*.xml"
       -- "/**/*.pdb"
       |> Zip deployDirVersioned (sprintf "%s/BankOCRKata.%s.zip" deployDir version))

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

"CleanDeploy"
==> "DeployLibrary"

"DeployLibrary"
==> "DeployConsole"

"DeployLibrary"
==> "DeployWPF"

// Deploy as Click once application
"DeployWPF"
==> "PruneDeploy"
==> "DeployClickOnce"

// Create a ZIP file
"DeployWPF"
==> "Zip"

RunTargetOrDefault "Default"
