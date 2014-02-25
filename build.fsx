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

Target "RunTests" (fun _ ->
    !! (buildDir + "xunit.dll")
       |> xUnit (fun p -> { p with Verbose = true
                                   WorkingDir = buildDir
                                   ShadowCopy = false
                                   XmlOutput = true
                                   OutputDir = buildDir }))

Target "DeployClickOnce" (fun _ ->
    let mage = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\mage.exe" // TODO
    let appManifest = sprintf "%s/%s.exe.manifest" deployDirVersioned appName
    let depManifest = sprintf "%s/%s.application" deployDir appName

    MageRun({ MageParams.ApplicationFile = depManifest
              MageParams.CertFile = Some(cert)
              MageParams.CodeBase = None
              MageParams.FromDirectory = deployDirVersioned
              MageParams.IconFile = ""
              MageParams.IconPath = ""
              MageParams.IncludeProvider = None
              MageParams.Install = Some(true)
              MageParams.Manifest = appManifest
              MageParams.Name = appName
              MageParams.Password = None
              MageParams.Processor = MageProcessor.X86
              MageParams.ProjectFiles = []
              MageParams.ProviderURL = ""
              MageParams.Publisher = Some(publisher)
              MageParams.SupportURL = None
              MageParams.TmpCertFile = ""
              MageParams.ToolsPath = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\"
              MageParams.TrustLevel = None
              MageParams.UseManifest = None
              MageParams.Version = version }))

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

"CleanDeploy"
==> "DeployLibrary"

"DeployLibrary"
==> "DeployConsole"

"DeployLibrary"
==> "DeployWPF"

// Deploy as Click once application
"DeployWPF"
==> "DeployClickOnce"

RunTargetOrDefault "Default"
