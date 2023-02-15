namespace Fun.AspNetCore

open System
open Microsoft.OpenApi.Models
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Routing
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.RateLimiting


type EndpointsCEBuilder(pattern: string) =

    member _.Pattern = pattern
    member val UseCustomizedTag = false with get, set


    member inline this.Run([<InlineIfLambda>] build: BuildEndpoints) =
        BuildGroup(fun endpoints ->
            let group = endpoints.MapGroup(this.Pattern)
            if not (String.IsNullOrEmpty this.Pattern) && not this.UseCustomizedTag then
                group.WithTags [| this.Pattern |] |> ignore
            build.Invoke(group)
        )


    member inline _.Zero() = BuildEndpoints(fun x -> x)

    member inline _.Yield(_: unit) = BuildEndpoints(fun x -> x)
    member inline _.Yield([<InlineIfLambda>] build: BuildEndpoints) = BuildEndpoints(fun g -> build.Invoke(g))
    member inline _.Yield([<InlineIfLambda>] build: BuildRoute) =
        BuildEndpoints(fun g ->
            build.Invoke(g) |> ignore
            g
        )
    member inline _.Yield([<InlineIfLambda>] build: BuildGroup) = BuildEndpoints(fun g -> build.Invoke(g))
    member inline _.Delay([<InlineIfLambda>] fn: unit -> BuildEndpoints) = BuildEndpoints(fun g -> fn().Invoke(g))


    member inline _.Combine([<InlineIfLambda>] build1: BuildEndpoints, [<InlineIfLambda>] build2: BuildEndpoints) =
        BuildEndpoints(fun group ->
            build1.Invoke(group) |> ignore
            build2.Invoke(group) |> ignore
            group
        )

    member inline _.For([<InlineIfLambda>] builder: BuildEndpoints, [<InlineIfLambda>] fn: unit -> BuildEndpoints) =
        BuildEndpoints(fun g -> fn().Invoke(builder.Invoke(g)))


    [<CustomOperation "set">]
    member inline _.set([<InlineIfLambda>] build: BuildEndpoints, [<InlineIfLambda>] fn: RouteGroupBuilder -> RouteGroupBuilder) =
        BuildEndpoints(fun endpoints -> build.Invoke(endpoints) |> fn)


    [<CustomOperation "name">]
    member inline _.name([<InlineIfLambda>] build: BuildEndpoints, name: string) = BuildEndpoints(fun group -> build.Invoke(group).WithName(name))

    [<CustomOperation "displayName">]
    member inline _.displayName([<InlineIfLambda>] build: BuildEndpoints, name: string) =
        BuildEndpoints(fun group -> build.Invoke(group).WithDisplayName(name))

    [<CustomOperation "groupName">]
    member inline _.groupName([<InlineIfLambda>] build: BuildEndpoints, name: string) =
        BuildEndpoints(fun group -> build.Invoke(group).WithGroupName(name).WithTags())

    [<CustomOperation "description">]
    member inline _.description([<InlineIfLambda>] build: BuildEndpoints, description: string) =
        BuildEndpoints(fun group -> build.Invoke(group).WithDescription(description))

    [<CustomOperation "summary">]
    member inline _.summary([<InlineIfLambda>] build: BuildEndpoints, summary: string) =
        BuildEndpoints(fun group -> build.Invoke(group).WithSummary(summary))

    [<CustomOperation "tags">]
    member inline this.tags([<InlineIfLambda>] build: BuildEndpoints, [<ParamArray>] tags: string[]) =
        this.UseCustomizedTag <- true
        BuildEndpoints(fun group -> build.Invoke(group).WithTags(tags))


    [<CustomOperation "openApi">]
    member inline _.openApi([<InlineIfLambda>] build: BuildEndpoints) = BuildEndpoints(fun group -> build.Invoke(group).WithOpenApi())

    [<CustomOperation "openApi">]
    member inline _.openApi([<InlineIfLambda>] build: BuildEndpoints, [<InlineIfLambda>] configOperation: OpenApiOperation -> unit) =
        BuildEndpoints(fun group ->
            build
                .Invoke(group)
                .WithOpenApi(fun x ->
                    configOperation x
                    x
                )
        )


    [<CustomOperation "anonymous">]
    member inline _.anonymous([<InlineIfLambda>] build: BuildEndpoints) = BuildEndpoints(fun group -> build.Invoke(group).AllowAnonymous())


    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoints) = BuildEndpoints(fun group -> build.Invoke(group).RequireAuthorization())

    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoints, policyNames: string[]) =
        BuildEndpoints(fun group -> build.Invoke(group).RequireAuthorization(policyNames))

    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoints, policy: AuthorizationPolicy) =
        BuildEndpoints(fun group -> build.Invoke(group).RequireAuthorization(policy))

    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoints, [<InlineIfLambda>] configurePolicy: AuthorizationPolicyBuilder -> unit) =
        BuildEndpoints(fun group -> build.Invoke(group).RequireAuthorization(fun x -> configurePolicy x))

    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoints, [<ParamArray>] authorizeData: IAuthorizeData[]) =
        BuildEndpoints(fun group -> build.Invoke(group).RequireAuthorization(authorizeData))


    [<CustomOperation "rateLimiting">]
    member inline _.rateLimiting([<InlineIfLambda>] build: BuildEndpoints, policy: IRateLimiterPolicy<_>) =
        BuildEndpoints(fun group -> build.Invoke(group).RequireRateLimiting(policy))

    [<CustomOperation "rateLimiting">]
    member inline _.rateLimiting([<InlineIfLambda>] build: BuildEndpoints, policyName: string) =
        BuildEndpoints(fun group -> build.Invoke(group).RequireRateLimiting(policyName))

    [<CustomOperation "disableRateLimiting">]
    member inline _.disableRateLimiting([<InlineIfLambda>] build: BuildEndpoints) =
        BuildEndpoints(fun group -> build.Invoke(group).DisableRateLimiting())
