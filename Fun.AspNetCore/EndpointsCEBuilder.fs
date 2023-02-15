namespace Fun.AspNetCore

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Routing
open Fun.AspNetCore.Internal


type EndpointsCEBuilder(pattern: string) =
    
    member _.Run(build: BuildEndpoints) =
        BuildEndpoints(fun endpoints -> build.Invoke(endpoints.MapGroup(pattern)))

    member inline _.Yield(_: unit) = BuildEndpoints(fun x -> x)
    member inline _.Yield([<InlineIfLambda>] x: BuildRoute) = x
    member inline _.Yield([<InlineIfLambda>] x: BuildEndpoints) = x
        
    member inline _.Delay([<InlineIfLambda>] fn: unit -> BuildEndpoints) = BuildEndpoints(fun x -> fn().Invoke x)
    member inline _.Delay([<InlineIfLambda>] fn: unit -> BuildRoute) = BuildEndpoints(fun x -> fn().Invoke(x) |> ignore; x)


    member inline _.Combine([<InlineIfLambda>] buildRoute: BuildRoute, [<InlineIfLambda>] builder: BuildEndpoints) =
        BuildEndpoints(fun x -> 
            let endpoints = builder.Invoke(x)
            buildRoute.Invoke(endpoints) |> ignore
            endpoints
        )

    member inline _.For([<InlineIfLambda>] builder: BuildEndpoints, [<InlineIfLambda>] fn: unit -> BuildEndpoints) =
        BuildEndpoints(fun x -> fn().Invoke(builder.Invoke(x)))


    [<CustomOperation "config">]
    member inline _.config([<InlineIfLambda>] build: BuildEndpoints, [<InlineIfLambda>] fn: RouteGroupBuilder -> RouteGroupBuilder) = 
        BuildEndpoints(fun endpoints -> build.Invoke(endpoints) |> fn)


    [<CustomOperation "GET">]
    member inline this.GET([<InlineIfLambda>] build: BuildEndpoints, pattern: string, handler: Func<_, _>) = 
        let buildRoute = EndpointCEBuilder (GET, pattern) { handle handler }
        this.Combine(buildRoute, build)
