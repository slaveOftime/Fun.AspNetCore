#r "nuget: Fun.Build, 0.5.0"
#r "nuget: NBomber, 5.2.1"

open System
open System.Net.Http
open Fun.Build
open NBomber.FSharp
open NBomber.Contracts

let options = {|
    GithubAction = EnvArg.Create("GITHUB_ACTION", description = "Run only in in github action container")
    NugetAPIKey = EnvArg.Create("NUGET_API_KEY", description = "Nuget api key")
|}


let stage_checkEnv =
    stage "Check environment" {
        run "dotnet tool restore"
        run (fun ctx -> printfn $"""GITHUB_ACTION: {ctx.GetEnvVar options.GithubAction.Name}""")
    }

let stage_lint =
    stage "Lint" {
        stage "Format" {
            whenNot { envVar options.GithubAction }
            run "dotnet fantomas . -r"
        }
        stage "Check" {
            whenEnvVar options.GithubAction
            run "dotnet fantomas . -r --check"
        }
    }

let stage_test = stage "Run unit tests" { run "dotnet test" }

let stage_benchmark name (url: string) =
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

            // so we can stop the server and end the stage
            ctx.SoftCancelStage()

            return result |> Result.map ignore
        })
    }


pipeline "deploy" {
    description "Build and deploy to nuget"
    stage_checkEnv
    stage_lint
    stage_test
    stage "Build packages" {
        run "dotnet pack -c Release Fun.AspNetCore/Fun.AspNetCore.fsproj -o ."
        run "dotnet pack -c Release Fun.AspNetCore.Blazor/Fun.AspNetCore.Blazor.fsproj -o ."
    }
    stage "Publish packages to nuget" {
        failIfIgnored
        whenBranch "master"
        whenEnvVar options.NugetAPIKey
        run (fun ctx ->
            let key = ctx.GetCmdArgOrEnvVar options.NugetAPIKey.Name
            ctx.RunSensitiveCommand $"""dotnet nuget push *.nupkg -s https://api.nuget.org/v3/index.json --skip-duplicate -k {key}"""
        )
    }
    runIfOnlySpecified
}

pipeline "test" {
    description "Format code and run tests"
    stage_checkEnv
    stage_lint
    stage_test
    runIfOnlySpecified
}

pipeline "benchmark" {
    description "Compare normal minimal apis with apis build by Fun.AspNetCore"
    workingDir "Fun.AspNetCore.Demo"
    stage "prepare" { run "dotnet build -c Release" }
    stage_benchmark "NormalMinimal" "https://localhost:51833/normal/hi"
    stage "pause" { run (Async.Sleep 10_000) }
    stage_benchmark "Fun.AspNetCore" "https://localhost:51833/api/hi"
    runIfOnlySpecified
}

tryPrintPipelineCommandHelp ()
