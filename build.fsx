#r "nuget: Fun.Build, 0.3.5"
#r "nuget: NBomber"

open System
open System.Net.Http
open Fun.Build
open NBomber.FSharp
open NBomber.Contracts


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

let benchmarkStage name (url: string) =
    stage name {
        paralle
        stage "server" {
            noStdRedirectForStep
            run "dotnet run -c Release --no-build"
        }
        run (fun ctx -> async {
            do! Async.Sleep 10_000

            let httpClient = new HttpClient()
            let result =
                Scenario.create (
                    name,
                    fun _ -> task {
                        let! response = httpClient.GetAsync(url)
                        return Response.ok (statusCode = string response.StatusCode)
                    }
                )
                |> Scenario.withoutWarmUp
                |> Scenario.withLoadSimulations [ LoadSimulation.KeepConstant(10, during = TimeSpan.FromSeconds(120)) ]
                |> NBomberRunner.registerScenario
                |> NBomberRunner.run

            ctx.SoftCancelStage()

            return result |> Result.map ignore
        })
    }


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

pipeline "benchmark" {
    description "Compare normal minimal apis with apis build by Fun.AspNetCore"
    noPrefixForStep
    workingDir "Fun.AspNetCore.Demo"
    stage "prepare" { run "dotnet build -c Release" }
    benchmarkStage "NormalMinimal" "https://localhost:51833/normal/hi"
    stage "pause" { run (Async.Sleep 10_000) }
    benchmarkStage "Fun.AspNetCore" "https://localhost:51833/api/hi"
    runIfOnlySpecified
}

tryPrintPipelineCommandHelp ()
