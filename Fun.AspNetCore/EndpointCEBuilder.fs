namespace Fun.AspNetCore

open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Fun.AspNetCore.Internal


type EndpointCEBuilder(method: SupportedHttpMethod, pattern: string) =
    
    member inline _.Build([<InlineIfLambda>] build: BuildEndpoint, method, pattern, handler: Delegate) =
        BuildRoute(fun group ->
            build.Invoke(
                match method with
                | GET -> group.MapGet(pattern, handler)
                | PUT -> group.MapPut(pattern, handler)
                | POST -> group.MapPost(pattern, handler)
                | DELETE -> group.MapDelete(pattern, handler)
                | PATCH -> group.MapPatch(pattern, handler)
            )
        )


    member inline _.Run([<InlineIfLambda>] x: BuildRoute) = x

    member inline _.Yield(_: unit) = BuildEndpoint(fun x -> x)
        
    member inline _.Delay([<InlineIfLambda>] fn: unit -> BuildEndpoint) = BuildEndpoint(fun x -> fn().Invoke x)
    member inline _.Delay([<InlineIfLambda>] fn: unit -> BuildRoute) = BuildRoute(fun x -> fn().Invoke(x))


    [<CustomOperation "config">]
    member inline _.config([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] fn: RouteHandlerBuilder -> RouteHandlerBuilder) =
        BuildEndpoint(fun route -> build.Invoke(route) |> fn)


    [<CustomOperation "produces">]
    member inline _.produces([<InlineIfLambda>] build: BuildEndpoint, ty: Type, ?statusCode: int, ?contentType: string) =
        BuildEndpoint(fun route ->
            build.Invoke(route).Produces(defaultArg statusCode 200, responseType = ty, contentType = defaultArg contentType "application/json")
        )

    [<CustomOperation "produces">]
    member inline _.produces([<InlineIfLambda>] build: BuildEndpoint, statusCode: int, ?contentType: string) =
        BuildEndpoint(fun route ->
            build.Invoke(route).Produces(statusCode, defaultArg contentType "application/json")
        )

    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoint) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireAuthorization())


    [<CustomOperation "handle">]
    member this.handle(build: BuildEndpoint, handler: Func<_, _>) = this.Build(build, method, pattern, handler)

    [<CustomOperation "handle">]
    member this.handle(build: BuildEndpoint, handler: Func<_, _, _>) = this.Build(build, method, pattern, handler)

    [<CustomOperation "handle">]
    member this.handle( build: BuildEndpoint, handler: Func<_, _, _, _>) = this.Build(build, method, pattern, handler)

    [<CustomOperation "handle">]
    member this.handle(build: BuildEndpoint, handler: Func<_, _, _, _, _>) = this.Build(build, method, pattern, handler)

    [<CustomOperation "handle">]
    member this.handle(build: BuildEndpoint, handler: Func<_, _, _, _, _, _>) = this.Build(build, method, pattern, handler)

    [<CustomOperation "handle">]
    member this.handle(build: BuildEndpoint, handler: Func<_, _, _, _, _, _, _>) = this.Build(build, method, pattern, handler)
