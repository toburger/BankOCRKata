#r @"packages/FAKE/tools/FakeLib.dll"
#r "System.Xml.Linq"
open System
open Fake

RestorePackages()

let version = "0.1.0.0"
let appName = "BankOCRKata.App"
let publisher = "Tobias Burger"
let cert = "./cert.pfx"

let buildDir = "./build/"

let timeout = TimeSpan.FromSeconds 15.

let mage = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\mage.exe" // TODO
let appManifest = sprintf "%s/%s.exe.manifest" buildDir appName
let depManifest = sprintf "%s/%s.application" buildDir appName

Target "Clean" (fun _ ->
    CleanDir buildDir)

Target "Prune" (fun _ ->
    !! (buildDir + "*.pdb")
    ++ (buildDir + "*.xml")
    |> DeleteFiles)

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

Target "AppDeployExt" (fun _ ->
    !! (buildDir + "/**/*.dll")
    ++ (buildDir + "/**/*.exe")
    ++ (buildDir + "/**/*.config")
    |> Seq.iter (fun f -> f |> Rename (f + ".deploy")))

Target "CreateAppManifest" (fun _ ->
    let args =
        sprintf """ -New Application -ToFile "%s" -Name %s -Version "%s" -Processor x86 -FromDirectory "%s" """
                appManifest
                appName
                version
                buildDir
    ExecProcess (fun pi -> 
        pi.FileName <- mage
        pi.Arguments <- args) timeout
    |> ignore)

Target "SignAppManifest" (fun _ ->
    let args =
        sprintf """ -Sign "%s" -CertFile "%s" """
                appManifest
                cert
    ExecProcess (fun pi ->
        pi.FileName <- mage
        pi.Arguments <- args) timeout
    |> ignore)

Target "CreateDeploymentManifest" (fun _ ->
    let args =
        sprintf """ -New Deployment -Version "%s" -Processor x86 -Install true -Publisher "%s" -AppManifest "%s" -ToFile "%s" """
                version
                publisher
                appManifest
                depManifest
    ExecProcess (fun pi ->
        pi.FileName <- mage
        pi.Arguments <- args) timeout
    |> ignore)

Target "UpdateDeploymentManifest" (fun _ ->
    let doc = System.Xml.Linq.XDocument.Load(depManifest)
    match doc.Root.Elements() |> Seq.tryPick (fun e -> if e.Name.LocalName = "deployment" then Some e else None) with
    | Some depNode ->
        let trustAttr = System.Xml.Linq.XName.Get("trustURLParameters", "")
        let depExtAttr = System.Xml.Linq.XName.Get("mapFileExtensions", "")
        depNode.SetAttributeValue(trustAttr, "true")
        depNode.SetAttributeValue(depExtAttr, "true")
        doc.Save(depManifest)
    | None -> targetError "UpdateDeploymentManifest" (exn (sprintf "no deployment element found in %s" depManifest)))

Target "SignDeploymentManifest" (fun _ ->
    let args =
        sprintf """ -Sign "%s" -CertFile "%s" """
                depManifest
                cert
    ExecProcess (fun pi ->
        pi.FileName <- mage
        pi.Arguments <- args) timeout
    |> ignore)

Target "BuildClickOnce" id

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

"BuildWPF"
==> "Prune"
==> "CreateAppManifest"
==> "SignAppManifest"
==> "AppDeployExt"
==> "CreateDeploymentManifest"
==> "UpdateDeploymentManifest"
==> "SignDeploymentManifest"
==> "BuildClickOnce"

RunTargetOrDefault "Default"
