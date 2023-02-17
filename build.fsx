#r "nuget: Fun.Build, 0.3.2"
#r "nuget: Fake.IO.FileSystem, 5.23.0"

open Fake.IO
open Fake.IO.Globbing.Operators
open Fun.Build


let envCheckStage =
    stage "Check environment" {
        paralle
        run "dotnet --version"
        run "dotnet --list-sdks"
        run "dotnet tool restore"
        run (fun ctx -> printfn $"""GITHUB_ACTION: {ctx.GetEnvVar "GITHUB_ACTION"}""")
    }

let lintStage =
    stage "Lint" {
        stage "Format" {
            whenNot { envVar "GITHUB_ACTION" }
            run "dotnet fantomas . -r"
        }
        stage "Check" {
            whenEnvVar "GITHUB_ACTION"
            run "dotnet fantomas . -r --check"
        }
    }

let testStage = stage "Run unit tests" { run "dotnet test" }


pipeline "deploy" {
    description "Build and deploy to nuget"
    noPrefixForStep
    envCheckStage
    lintStage
    testStage
    stage "Build packages" {
        run "dotnet pack -c Release Fun.AspNetCore/Fun.AspNetCore.fsproj -o ."
        run "dotnet pack -c Release Fun.AspNetCore.Blazor/Fun.AspNetCore.Blazor.fsproj -o ."
    }
    stage "Publish packages to nuget" {
        failIfIgnored
        whenAll {
            branch "master"
            whenAny {
                envVar "NUGET_API_KEY"
                cmdArg "NUGET_API_KEY"
            }
        }
        run (fun ctx ->
            let key = ctx.GetCmdArgOrEnvVar "NUGET_API_KEY"
            ctx.RunSensitiveCommand $"""dotnet nuget push *.nupkg -s https://api.nuget.org/v3/index.json --skip-duplicate -k {key}"""
        )
    }
    runIfOnlySpecified
}

pipeline "test" {
    description "Format code and run tests"
    noPrefixForStep
    envCheckStage
    lintStage
    testStage
    runIfOnlySpecified
}


tryPrintPipelineCommandHelp ()
