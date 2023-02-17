namespace Fun.AspNetCore

open System
open Microsoft.OpenApi.Models
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.RateLimiting
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.OutputCaching


type EndpointCEBuilder(methods: string list, pattern: string) =

    static member val GetMethods = [ HttpMethods.Get ]
    static member val PutMethods = [ HttpMethods.Put ]
    static member val PostMethods = [ HttpMethods.Post ]
    static member val DeleteMethods = [ HttpMethods.Delete ]
    static member val PatchMethods = [ HttpMethods.Patch ]


    member _.Methods = methods
    member _.Pattern = pattern

    member inline this.Build([<InlineIfLambda>] build: BuildEndpoint, handler: Delegate) =
        BuildRoute(fun group ->
            let route = group.MapMethods(this.Pattern, this.Methods, handler)
            build.Invoke(route)
        )


    member inline _.Run([<InlineIfLambda>] fn: BuildRoute) = fn //BuildRoute(fun x -> fn.Invoke(x)

    member inline _.Zero() = BuildEndpoint(fun x -> x)


    member inline _.Yield(_: unit) = BuildEndpoint(fun x -> x)
    member inline _.Delay([<InlineIfLambda>] fn: unit -> BuildEndpoint) = BuildEndpoint(fun x -> fn().Invoke x)

    member inline this.Yield(x: IResult) = BuildRoute(fun group -> group.MapMethods(this.Pattern, this.Methods, Func<_>(fun () -> x)))
    member inline _.Delay([<InlineIfLambda>] fn: unit -> BuildRoute) = BuildRoute(fun r -> fn().Invoke(r))
    member inline _.Combine([<InlineIfLambda>] buildEndpoint: BuildEndpoint, [<InlineIfLambda>] buildRoute: BuildRoute) =
        BuildRoute(fun group ->
            let route = buildRoute.Invoke(group)
            buildEndpoint.Invoke(route) |> ignore
            route
        )


    member inline this.For([<InlineIfLambda>] builder: BuildEndpoint, [<InlineIfLambda>] fn: unit -> BuildRoute) = this.Combine(builder, fn ())


    [<CustomOperation "set">]
    member inline _.set([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] fn: RouteHandlerBuilder -> RouteHandlerBuilder) =
        BuildEndpoint(fun route -> fn (build.Invoke(route)))


    [<CustomOperation "name">]
    member inline _.name([<InlineIfLambda>] build: BuildEndpoint, name: string) = BuildEndpoint(fun route -> build.Invoke(route).WithName(name))

    [<CustomOperation "displayName">]
    member inline _.displayName([<InlineIfLambda>] build: BuildEndpoint, name: string) =
        BuildEndpoint(fun route -> build.Invoke(route).WithDisplayName(name))

    [<CustomOperation "groupName">]
    member inline _.groupName([<InlineIfLambda>] build: BuildEndpoint, name: string) =
        BuildEndpoint(fun route -> build.Invoke(route).WithGroupName(name))

    [<CustomOperation "description">]
    member inline _.description([<InlineIfLambda>] build: BuildEndpoint, description: string) =
        BuildEndpoint(fun route -> build.Invoke(route).WithDescription(description))

    [<CustomOperation "summary">]
    member inline _.summary([<InlineIfLambda>] build: BuildEndpoint, summary: string) =
        BuildEndpoint(fun route -> build.Invoke(route).WithSummary(summary))

    [<CustomOperation "tags">]
    member inline _.tags([<InlineIfLambda>] build: BuildEndpoint, [<ParamArray>] tags: string[]) =
        BuildEndpoint(fun route -> build.Invoke(route).WithTags(tags))


    [<CustomOperation "metas">]
    member inline _.metas([<InlineIfLambda>] build: BuildEndpoint, metas: obj[]) = BuildEndpoint(fun route -> build.Invoke(route).WithMetadata(metas))


    [<CustomOperation "produces">]
    member inline _.produces([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] getType: unit -> Type, statusCode) =
        BuildEndpoint(fun route -> build.Invoke(route).Produces(statusCode, responseType = getType (), contentType = "application/json"))

    [<CustomOperation "produces">]
    member inline _.produces([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] getType: unit -> Type, statusCode, contentType) =
        BuildEndpoint(fun route -> build.Invoke(route).Produces(statusCode, responseType = getType (), contentType = contentType))

    [<CustomOperation "produces">]
    member inline _.produces([<InlineIfLambda>] build: BuildEndpoint, statusCode, contentType) =
        BuildEndpoint(fun route -> build.Invoke(route).Produces(statusCode, contentType))

    [<CustomOperation "producesProblem">]
    member inline _.producesProblem([<InlineIfLambda>] build: BuildEndpoint, statusCode) =
        BuildEndpoint(fun route -> build.Invoke(route).ProducesProblem(statusCode))

    [<CustomOperation "producesProblem">]
    member inline _.producesProblem([<InlineIfLambda>] build: BuildEndpoint, statusCode, contentType) =
        BuildEndpoint(fun route -> build.Invoke(route).ProducesProblem(statusCode, contentType))

    [<CustomOperation "producesValidationProblem">]
    member inline _.producesValidationProblem([<InlineIfLambda>] build: BuildEndpoint) =
        BuildEndpoint(fun route -> build.Invoke(route).ProducesValidationProblem())

    [<CustomOperation "producesValidationProblem">]
    member inline _.producesValidationProblem([<InlineIfLambda>] build: BuildEndpoint, statusCode) =
        BuildEndpoint(fun route -> build.Invoke(route).ProducesValidationProblem(statusCode))

    [<CustomOperation "producesValidationProblem">]
    member inline _.producesValidationProblem([<InlineIfLambda>] build: BuildEndpoint, statusCode, contentType) =
        BuildEndpoint(fun route -> build.Invoke(route).ProducesValidationProblem(statusCode, contentType))


    [<CustomOperation "openApi">]
    member inline _.openApi([<InlineIfLambda>] build: BuildEndpoint) = BuildEndpoint(fun route -> build.Invoke(route).WithOpenApi())

    [<CustomOperation "openApi">]
    member inline _.openApi([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] configOperation: OpenApiOperation -> unit) =
        BuildEndpoint(fun route ->
            build
                .Invoke(route)
                .WithOpenApi(fun x ->
                    configOperation x
                    x
                )
        )


    [<CustomOperation "anonymous">]
    member inline _.anonymous([<InlineIfLambda>] build: BuildEndpoint) = BuildEndpoint(fun route -> build.Invoke(route).AllowAnonymous())

    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoint) = BuildEndpoint(fun route -> build.Invoke(route).RequireAuthorization())

    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoint, policyNames: string[]) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireAuthorization(policyNames))

    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoint, policy: AuthorizationPolicy) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireAuthorization(policy))

    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] configurePolicy: AuthorizationPolicyBuilder -> unit) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireAuthorization(fun x -> configurePolicy x))

    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoint, [<ParamArray>] authorizeData: IAuthorizeData[]) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireAuthorization(authorizeData))


    [<CustomOperation "rateLimiting">]
    member inline _.rateLimiting([<InlineIfLambda>] build: BuildEndpoint, policy: IRateLimiterPolicy<_>) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireRateLimiting(policy))

    [<CustomOperation "rateLimiting">]
    member inline _.rateLimiting([<InlineIfLambda>] build: BuildEndpoint, policyName: string) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireRateLimiting(policyName))

    [<CustomOperation "disableRateLimiting">]
    member inline _.disableRateLimiting([<InlineIfLambda>] build: BuildEndpoint) =
        BuildEndpoint(fun route -> build.Invoke(route).DisableRateLimiting())


    [<CustomOperation "cacheOutput">]
    member inline _.cacheOutput([<InlineIfLambda>] build: BuildEndpoint) = BuildEndpoint(fun route -> build.Invoke(route).CacheOutput())

    [<CustomOperation "cacheOutput">]
    member inline _.cacheOutput([<InlineIfLambda>] build: BuildEndpoint, policyName: string) =
        BuildEndpoint(fun route -> build.Invoke(route).CacheOutput(policyName))

    [<CustomOperation "cacheOutput">]
    member inline _.cacheOutput([<InlineIfLambda>] build: BuildEndpoint, policy: IOutputCachePolicy) =
        BuildEndpoint(fun route -> build.Invoke(route).CacheOutput(policy))

    [<CustomOperation "cacheOutput">]
    member inline _.cacheOutput([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] configurePolicy: OutputCachePolicyBuilder -> OutputCachePolicyBuilder) =
        BuildEndpoint(fun route -> build.Invoke(route).CacheOutput(fun x -> configurePolicy x |> ignore))


    [<CustomOperation "handle">]
    member inline this.handle([<InlineIfLambda>] build: BuildEndpoint, handler: Func<'T, _>) =
        if typeof<'T> = typeof<unit> then
            this.Build(build, Func<_>(fun () -> handler.Invoke(unbox<'T> ())))
        else
            this.Build(build, handler)

    [<CustomOperation "handle">]
    member inline this.handle([<InlineIfLambda>] build: BuildEndpoint, handler: Func<_, _, _>) = this.Build(build, handler)

    [<CustomOperation "handle">]
    member inline this.handle([<InlineIfLambda>] build: BuildEndpoint, handler: Func<_, _, _, _>) = this.Build(build, handler)

    [<CustomOperation "handle">]
    member inline this.handle([<InlineIfLambda>] build: BuildEndpoint, handler: Func<_, _, _, _, _>) = this.Build(build, handler)

    [<CustomOperation "handle">]
    member inline this.handle([<InlineIfLambda>] build: BuildEndpoint, handler: Func<_, _, _, _, _, _>) = this.Build(build, handler)

    [<CustomOperation "handle">]
    member inline this.handle([<InlineIfLambda>] build: BuildEndpoint, handler: Func<_, _, _, _, _, _, _>) = this.Build(build, handler)
