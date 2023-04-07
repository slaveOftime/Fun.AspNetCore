namespace Fun.AspNetCore

open System
open Microsoft.OpenApi.Models
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Routing
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.RateLimiting
open Microsoft.AspNetCore.Cors.Infrastructure
open Fun.AspNetCore.Internal


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


    /// Give a function to configure the endpoints.
    [<CustomOperation "set">]
    member inline _.set([<InlineIfLambda>] build: BuildEndpoints, [<InlineIfLambda>] fn: RouteGroupBuilder -> RouteGroupBuilder) =
        BuildEndpoints(fun endpoints -> build.Invoke(endpoints) |> fn)


    /// Registers a filter onto the route handler.
    [<CustomOperation "filter">]
    member inline _.filter([<InlineIfLambda>] build: BuildEndpoints, filter: IEndpointFilter) =
        BuildEndpoints(fun route -> build.Invoke(route).AddEndpointFilter(filter))


    /// Adds the Microsoft.AspNetCore.Routing.IExcludeFromDescriptionMetadata to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "excludeFromDescription">]
    member inline _.excludeFromDescription([<InlineIfLambda>] build: BuildEndpoints) =
        BuildEndpoints(fun route -> build.Invoke(route).ExcludeFromDescription())


    /// Adds a CORS policy with the specified name to the endpoint(s).
    [<CustomOperation "requiredCors">]
    member inline _.requiredCors([<InlineIfLambda>] build: BuildEndpoints, policyName: string) =
        BuildEndpoints(fun route -> build.Invoke(route).RequireCors(policyName))

    /// Adds the specified CORS policy to the endpoint(s).
    [<CustomOperation "requiredCors">]
    member inline _.requiredCors([<InlineIfLambda>] build: BuildEndpoints, [<InlineIfLambda>] config: CorsPolicyBuilder -> CorsPolicyBuilder) =
        BuildEndpoints(fun route -> build.Invoke(route).RequireCors(fun x -> config x |> ignore))


    /// Requires that endpoints match one of the specified hosts during routing.
    [<CustomOperation "requireHost">]
    member inline _.requireHost([<InlineIfLambda>] build: BuildEndpoints, [<ParamArray>] hosts: string[]) =
        BuildEndpoints(fun route -> build.Invoke(route).RequireHost(hosts))


    /// Adds the Microsoft.AspNetCore.Routing.IEndpointNameMetadata to the Metadata collection for all endpoints produced on the target Microsoft.AspNetCore.Builder.IEndpointConventionBuilder given the endpointName. The Microsoft.AspNetCore.Routing.IEndpointNameMetadata on the endpoint is used for link generation and is treated as the operation ID in the given endpoint's OpenAPI specification.
    [<CustomOperation "name">]
    member inline _.name([<InlineIfLambda>] build: BuildEndpoints, name: string) = BuildEndpoints(fun group -> build.Invoke(group).WithName(name))

    /// Sets the Microsoft.AspNetCore.Builder.EndpointBuilder.DisplayName to the provided displayName for all builders created by builder.
    [<CustomOperation "displayName">]
    member inline _.displayName([<InlineIfLambda>] build: BuildEndpoints, name: string) =
        BuildEndpoints(fun group -> build.Invoke(group).WithDisplayName(name))

    /// Sets the Microsoft.AspNetCore.Routing.EndpointGroupNameAttribute for all endpoints produced on the target Microsoft.AspNetCore.Builder.IEndpointConventionBuilder given the endpointGroupName. The Microsoft.AspNetCore.Routing.IEndpointGroupNameMetadata on the endpoint is used to set the endpoint's GroupName in the OpenAPI specification.
    [<CustomOperation "groupName">]
    member inline _.groupName([<InlineIfLambda>] build: BuildEndpoints, name: string) =
        BuildEndpoints(fun group -> build.Invoke(group).WithGroupName(name).WithTags())

    /// Adds Microsoft.AspNetCore.Http.Metadata.IEndpointDescriptionMetadata to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "description">]
    member inline _.description([<InlineIfLambda>] build: BuildEndpoints, description: string) =
        BuildEndpoints(fun group -> build.Invoke(group).WithDescription(description))

    /// Adds Microsoft.AspNetCore.Http.Metadata.IEndpointSummaryMetadata to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "summary">]
    member inline _.summary([<InlineIfLambda>] build: BuildEndpoints, summary: string) =
        BuildEndpoints(fun group -> build.Invoke(group).WithSummary(summary))

    /// <summary>
    /// Adds the <see cref="T:Microsoft.AspNetCore.Http.Metadata.ITagsMetadata" /> to <see cref="P:Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata" /> for all endpoints
    /// produced by <paramref name="builder" />.
    /// </summary>
    /// <remarks>
    /// The OpenAPI specification supports a tags classification to categorize operations
    /// into related groups. These tags are typically included in the generated specification
    /// and are typically used to group operations by tags in the UI.
    /// </remarks>
    [<CustomOperation "tags">]
    member inline this.tags([<InlineIfLambda>] build: BuildEndpoints, [<ParamArray>] tags: string[]) =
        this.UseCustomizedTag <- true
        BuildEndpoints(fun group -> build.Invoke(group).WithTags(tags))


    /// Adds an OpenAPI annotation to Microsoft.AspNetCore.Http.Endpoint.Metadata associated with the current endpoint.
    [<CustomOperation "openApi">]
    member inline _.openApi([<InlineIfLambda>] build: BuildEndpoints) = BuildEndpoints(fun group -> build.Invoke(group).WithOpenApi())

    /// Adds an OpenAPI annotation to Microsoft.AspNetCore.Http.Endpoint.Metadata associated with the current endpoint.
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


    /// Allows anonymous access to the endpoint by adding Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute to the endpoint metadata. This will bypass all authorization checks for the endpoint including the default authorization policy and fallback authorization policy.
    [<CustomOperation "anonymous">]
    member inline _.anonymous([<InlineIfLambda>] build: BuildEndpoints) = BuildEndpoints(fun group -> build.Invoke(group).AllowAnonymous())

    /// Adds the default authorization policy to the endpoint(s).
    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoints) = BuildEndpoints(fun group -> build.Invoke(group).RequireAuthorization())

    /// Adds authorization policies with the specified names to the endpoint(s).
    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoints, policyNames: string[]) =
        BuildEndpoints(fun group -> build.Invoke(group).RequireAuthorization(policyNames))

    /// Adds an authorization policy to the endpoint(s).
    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoints, policy: AuthorizationPolicy) =
        BuildEndpoints(fun group -> build.Invoke(group).RequireAuthorization(policy))

    /// Adds an new authorization policy configured by a callback to the endpoint(s).
    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoints, [<InlineIfLambda>] configurePolicy: AuthorizationPolicyBuilder -> unit) =
        BuildEndpoints(fun group -> build.Invoke(group).RequireAuthorization(fun x -> configurePolicy x))

    /// Adds authorization policies with the specified Microsoft.AspNetCore.Authorization.IAuthorizeData to the endpoint(s).
    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoints, [<ParamArray>] authorizeData: IAuthorizeData[]) =
        BuildEndpoints(fun group -> build.Invoke(group).RequireAuthorization(authorizeData))

    /// Adds the specified rate limiting policy to the endpoint(s).
    [<CustomOperation "rateLimiting">]
    member inline _.rateLimiting([<InlineIfLambda>] build: BuildEndpoints, policy: IRateLimiterPolicy<_>) =
        BuildEndpoints(fun group -> build.Invoke(group).RequireRateLimiting(policy))

    /// Adds the specified rate limiting policy to the endpoint(s).
    [<CustomOperation "rateLimiting">]
    member inline _.rateLimiting([<InlineIfLambda>] build: BuildEndpoints, policyName: string) =
        BuildEndpoints(fun group -> build.Invoke(group).RequireRateLimiting(policyName))

    /// <summary>
    /// Disables rate limiting on the endpoint(s).
    /// </summary>
    /// <remarks>Will skip both the global limiter, and any endpoint-specific limiters that apply to the endpoint(s).</remarks>
    [<CustomOperation "disableRateLimiting">]
    member inline _.disableRateLimiting([<InlineIfLambda>] build: BuildEndpoints) =
        BuildEndpoints(fun group -> build.Invoke(group).DisableRateLimiting())
