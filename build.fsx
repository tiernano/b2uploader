//include fake lib
#r @"tools\FAKE\tools\Fakelib.dll"
open Fake
open System
open Fake.AssemblyInfoFile

let buildDir = @".\build"
let deployDir = @".\deploy"

let date = DateTime.UtcNow
let version = String.Format("{0}.{1}.{2}.{3:0.#}", date.Year, date.Month, date.Day, date.TimeOfDay.TotalMinutes.ToString("F0"))

let copyright = "TIernan OToole 2016"
let productName = "B2 Uploader"
let companyName = "Tiernan OToole"

let buildMode = getBuildParamOrDefault "buildMode" "Releasex64"
let setParams defaults =
        { defaults with
            Verbosity = Some(Quiet)
            Targets = ["Build"]
            Properties =
                [
                    "Optimize", "True"
                    "DebugSymbols", "True"
                    "Configuration", buildMode                    
                ]
         }



Target "SetAssemblyInfo" (fun _  ->

    CreateCSharpAssemblyInfo "B2Classes/Properties/AssemblyInfo.cs"
       [Attribute.Title "B2Classes"
        Attribute.Guid "fe353639-3b33-44de-9147-45b63818d8a7"
        Attribute.Product productName
        Attribute.Company companyName
        Attribute.Copyright copyright
        Attribute.Version version
        Attribute.FileVersion version
        ]

    CreateCSharpAssemblyInfo "B2Uploader/Properties/AssemblyInfo.cs"
       [Attribute.Title "B2Uploader"
        Attribute.Guid "a5d41169-c2ee-4b5e-a2d8-b63e485597a6"
        Attribute.Product productName
        Attribute.Company companyName
        Attribute.Copyright copyright
        Attribute.Version version
        Attribute.FileVersion version
        ]
)

RestorePackages()

Target "Clean" (fun _ ->
    CleanDir buildDir
)


Target "Classes" (fun _ ->
    !! @"B2Classes\B2Classes.csproj"
        |> MSBuildReleaseExt buildDir setParams.Properties "Build"
        |> Log "AppBuild-Output: "
)

Target "Uploader" (fun _ ->
    !! @"B2Uploader\B2Uploader.csproj"
        |> MSBuildReleaseExt buildDir setParams.Properties "Build"
        |> Log "AppBuild-Output: "
)

Target "Zip" (fun _ ->
    !! (buildDir + "\**\*.*")
        -- "*.zip"
        |> Zip buildDir (deployDir + "\B2Uploader." + version + ".zip")    
)

"Clean"
    ==> "SetAssemblyInfo"
    ==> "Classes"
    ==> "Uploader"
    ==> "Zip"

RunTargetOrDefault "Zip"