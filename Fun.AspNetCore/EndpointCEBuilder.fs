namespace Fun.AspNetCore

open System
open Microsoft.OpenApi.Models
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.RateLimiting
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.OutputCaching
open Microsoft.AspNetCore.Cors.Infrastructure
open Fun.AspNetCore.Internal


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


    member inline _.Run([<InlineIfLambda>] fn: BuildRoute) = BuildRoute(fun x -> fn.Invoke(x))

    member inline this.Run([<InlineIfLambda>] fn: BuildEndpoint) = this.Build(fn, Func<_>(fun () -> Results.Ok()))


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


    /// Give a function to configure the endpoint.
    [<CustomOperation "set">]
    member inline _.set([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] fn: RouteHandlerBuilder -> RouteHandlerBuilder) =
        BuildEndpoint(fun route -> fn (build.Invoke(route)))


    /// Registers a filter onto the route handler.
    [<CustomOperation "filter">]
    member inline _.filter([<InlineIfLambda>] build: BuildEndpoint, filter: IEndpointFilter) =
        BuildEndpoint(fun route -> build.Invoke(route).AddEndpointFilter(filter))


    /// Adds the Microsoft.AspNetCore.Routing.IExcludeFromDescriptionMetadata to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "excludeFromDescription">]
    member inline _.excludeFromDescription([<InlineIfLambda>] build: BuildEndpoint) =
        BuildEndpoint(fun route -> build.Invoke(route).ExcludeFromDescription())


    /// Adds a CORS policy with the specified name to the endpoint(s).
    [<CustomOperation "requiredCors">]
    member inline _.requiredCors([<InlineIfLambda>] build: BuildEndpoint, policyName: string) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireCors(policyName))

    /// Adds the specified CORS policy to the endpoint(s).
    [<CustomOperation "requiredCors">]
    member inline _.requiredCors([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] config: CorsPolicyBuilder -> CorsPolicyBuilder) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireCors(fun x -> config x |> ignore))


    /// Requires that endpoints match one of the specified hosts during routing.
    [<CustomOperation "requireHost">]
    member inline _.requireHost([<InlineIfLambda>] build: BuildEndpoint, [<ParamArray>] hosts: string[]) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireHost(hosts))


    /// Adds Microsoft.AspNetCore.Http.Metadata.IAcceptsMetadata to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "accepts">]
    member inline _.accepts([<InlineIfLambda>] build: BuildEndpoint, contentType: string, [<ParamArray>] additionalContentTypes: string[]) =
        BuildEndpoint(fun route -> build.Invoke(route).Accepts(contentType, additionalContentTypes))

    /// Adds Microsoft.AspNetCore.Http.Metadata.IAcceptsMetadata to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "accepts">]
    member inline _.accepts
        (
            [<InlineIfLambda>] build: BuildEndpoint,
            [<InlineIfLambda>] requestType: unit -> Type,
            contentType: string,
            [<ParamArray>] additionalContentTypes: string[]
        ) =
        BuildEndpoint(fun route -> build.Invoke(route).Accepts(requestType (), contentType, additionalContentTypes))

    /// Adds Microsoft.AspNetCore.Http.Metadata.IAcceptsMetadata to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "accepts">]
    member inline _.acceptsOptional([<InlineIfLambda>] build: BuildEndpoint, contentType: string, [<ParamArray>] additionalContentTypes: string[]) =
        BuildEndpoint(fun route -> build.Invoke(route).Accepts(true, contentType, additionalContentTypes))

    /// Adds Microsoft.AspNetCore.Http.Metadata.IAcceptsMetadata to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "accepts">]
    member inline _.acceptsOptional
        (
            [<InlineIfLambda>] build: BuildEndpoint,
            [<InlineIfLambda>] requestType: unit -> Type,
            contentType: string,
            [<ParamArray>] additionalContentTypes: string[]
        ) =
        BuildEndpoint(fun route -> build.Invoke(route).Accepts(requestType (), true, contentType, additionalContentTypes))


    /// Adds the Microsoft.AspNetCore.Routing.IEndpointNameMetadata to the Metadata collection for all endpoints produced on the target Microsoft.AspNetCore.Builder.IEndpointConventionBuilder given the endpointName. The Microsoft.AspNetCore.Routing.IEndpointNameMetadata on the endpoint is used for link generation and is treated as the operation ID in the given endpoint's OpenAPI specification.
    [<CustomOperation "name">]
    member inline _.name([<InlineIfLambda>] build: BuildEndpoint, name: string) = BuildEndpoint(fun route -> build.Invoke(route).WithName(name))

    /// Sets the Microsoft.AspNetCore.Builder.EndpointBuilder.DisplayName to the provided displayName for all builders created by builder.
    [<CustomOperation "displayName">]
    member inline _.displayName([<InlineIfLambda>] build: BuildEndpoint, name: string) =
        BuildEndpoint(fun route -> build.Invoke(route).WithDisplayName(name))

    /// Sets the Microsoft.AspNetCore.Routing.EndpointGroupNameAttribute for all endpoints produced on the target Microsoft.AspNetCore.Builder.IEndpointConventionBuilder given the endpointGroupName. The Microsoft.AspNetCore.Routing.IEndpointGroupNameMetadata on the endpoint is used to set the endpoint's GroupName in the OpenAPI specification.
    [<CustomOperation "groupName">]
    member inline _.groupName([<InlineIfLambda>] build: BuildEndpoint, name: string) =
        BuildEndpoint(fun route -> build.Invoke(route).WithGroupName(name))

    /// Adds Microsoft.AspNetCore.Http.Metadata.IEndpointDescriptionMetadata to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "description">]
    member inline _.description([<InlineIfLambda>] build: BuildEndpoint, description: string) =
        BuildEndpoint(fun route -> build.Invoke(route).WithDescription(description))

    /// Adds Microsoft.AspNetCore.Http.Metadata.IEndpointSummaryMetadata to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "summary">]
    member inline _.summary([<InlineIfLambda>] build: BuildEndpoint, summary: string) =
        BuildEndpoint(fun route -> build.Invoke(route).WithSummary(summary))

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
    member inline _.tags([<InlineIfLambda>] build: BuildEndpoint, [<ParamArray>] tags: string[]) =
        BuildEndpoint(fun route -> build.Invoke(route).WithTags(tags))


    /// Adds the provided metadata items to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all builders produced by builder.
    [<CustomOperation "metas">]
    member inline _.metas([<InlineIfLambda>] build: BuildEndpoint, metas: obj[]) = BuildEndpoint(fun route -> build.Invoke(route).WithMetadata(metas))


    /// Adds an Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "produces">]
    member inline _.produces([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] getType: unit -> Type, statusCode) =
        BuildEndpoint(fun route -> build.Invoke(route).Produces(statusCode, responseType = getType (), contentType = "application/json"))

    /// Adds an Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "produces">]
    member inline _.produces([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] getType: unit -> Type, statusCode, contentType) =
        BuildEndpoint(fun route -> build.Invoke(route).Produces(statusCode, responseType = getType (), contentType = contentType))

    /// Adds an Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "produces">]
    member inline _.produces([<InlineIfLambda>] build: BuildEndpoint, statusCode, contentType) =
        BuildEndpoint(fun route -> build.Invoke(route).Produces(statusCode, contentType))

    /// Adds an Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata with a Microsoft.AspNetCore.Mvc.ProblemDetails type to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "producesProblem">]
    member inline _.producesProblem([<InlineIfLambda>] build: BuildEndpoint, statusCode) =
        BuildEndpoint(fun route -> build.Invoke(route).ProducesProblem(statusCode))

    /// Adds an Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata with a Microsoft.AspNetCore.Mvc.ProblemDetails type to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "producesProblem">]
    member inline _.producesProblem([<InlineIfLambda>] build: BuildEndpoint, statusCode, contentType) =
        BuildEndpoint(fun route -> build.Invoke(route).ProducesProblem(statusCode, contentType))

    /// Adds an Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata with a Microsoft.AspNetCore.Http.HttpValidationProblemDetails type to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "producesValidationProblem">]
    member inline _.producesValidationProblem([<InlineIfLambda>] build: BuildEndpoint) =
        BuildEndpoint(fun route -> build.Invoke(route).ProducesValidationProblem())

    /// Adds an Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata with a Microsoft.AspNetCore.Http.HttpValidationProblemDetails type to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "producesValidationProblem">]
    member inline _.producesValidationProblem([<InlineIfLambda>] build: BuildEndpoint, statusCode) =
        BuildEndpoint(fun route -> build.Invoke(route).ProducesValidationProblem(statusCode))

    /// Adds an Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata with a Microsoft.AspNetCore.Http.HttpValidationProblemDetails type to Microsoft.AspNetCore.Builder.EndpointBuilder.Metadata for all endpoints produced by builder.
    [<CustomOperation "producesValidationProblem">]
    member inline _.producesValidationProblem([<InlineIfLambda>] build: BuildEndpoint, statusCode, contentType) =
        BuildEndpoint(fun route -> build.Invoke(route).ProducesValidationProblem(statusCode, contentType))

    /// Adds an OpenAPI annotation to Microsoft.AspNetCore.Http.Endpoint.Metadata associated with the current endpoint.
    [<CustomOperation "openApi">]
    member inline _.openApi([<InlineIfLambda>] build: BuildEndpoint) = BuildEndpoint(fun route -> build.Invoke(route).WithOpenApi())

    /// Adds an OpenAPI annotation to Microsoft.AspNetCore.Http.Endpoint.Metadata associated with the current endpoint.
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

    /// Allows anonymous access to the endpoint by adding Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute to the endpoint metadata. This will bypass all authorization checks for the endpoint including the default authorization policy and fallback authorization policy.
    [<CustomOperation "anonymous">]
    member inline _.anonymous([<InlineIfLambda>] build: BuildEndpoint) = BuildEndpoint(fun route -> build.Invoke(route).AllowAnonymous())

    /// Adds the default authorization policy to the endpoint(s).
    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoint) = BuildEndpoint(fun route -> build.Invoke(route).RequireAuthorization())

    /// Adds authorization policies with the specified names to the endpoint(s).
    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoint, policyNames: string[]) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireAuthorization(policyNames))

    /// Adds an authorization policy to the endpoint(s).
    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoint, policy: AuthorizationPolicy) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireAuthorization(policy))

    /// Adds an new authorization policy configured by a callback to the endpoint(s).
    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] configurePolicy: AuthorizationPolicyBuilder -> unit) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireAuthorization(fun x -> configurePolicy x))

    /// Adds authorization policies with the specified Microsoft.AspNetCore.Authorization.IAuthorizeData to the endpoint(s).
    [<CustomOperation "authorization">]
    member inline _.authorization([<InlineIfLambda>] build: BuildEndpoint, [<ParamArray>] authorizeData: IAuthorizeData[]) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireAuthorization(authorizeData))

    /// Adds the specified rate limiting policy to the endpoint(s).
    [<CustomOperation "rateLimiting">]
    member inline _.rateLimiting([<InlineIfLambda>] build: BuildEndpoint, policy: IRateLimiterPolicy<_>) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireRateLimiting(policy))

    /// Adds the specified rate limiting policy to the endpoint(s).
    [<CustomOperation "rateLimiting">]
    member inline _.rateLimiting([<InlineIfLambda>] build: BuildEndpoint, policyName: string) =
        BuildEndpoint(fun route -> build.Invoke(route).RequireRateLimiting(policyName))

    /// <summary>
    /// Disables rate limiting on the endpoint(s).
    /// </summary>
    /// <remarks>Will skip both the global limiter, and any endpoint-specific limiters that apply to the endpoint(s).</remarks>
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
    member inline _.cacheOutput
        (
            [<InlineIfLambda>] build: BuildEndpoint,
            [<InlineIfLambda>] configurePolicy: OutputCachePolicyBuilder -> OutputCachePolicyBuilder
        ) =
        BuildEndpoint(fun route -> build.Invoke(route).CacheOutput(fun x -> configurePolicy x |> ignore))


    [<CustomOperation "responseCache">]
    member inline _.responseCache([<InlineIfLambda>] build: BuildEndpoint, time: TimeSpan) =
        BuildEndpoint(fun route ->
            build
                .Invoke(route)
                .AddEndpointFilter(
                    { new IEndpointFilter with
                        member _.InvokeAsync(ctx, next) =
                            if ctx.HttpContext.Response.HasStarted then
                                failwith "Cannot set headers when they are already sent to client"
                            ctx.HttpContext.Response.Headers.CacheControl <- $"max-age:{time.TotalSeconds}"
                            next.Invoke ctx
                    }
                )
        )


    [<CustomOperation "handle">]
    member inline this.handle([<InlineIfLambda>] build: BuildEndpoint, handler: Func<'T, _>) =
        if typeof<'T> = typeof<unit> then
            this.Build(build, Func<_>(fun () -> handler.Invoke(unbox<'T> ())))
        else
            this.Build(build, handler)

    /// Provide a Func delegate for the endpoints. You can only call this one time.
    [<CustomOperation "handle">]
    member inline this.handle([<InlineIfLambda>] build: BuildEndpoint, handler: Func<_, _, _>) = this.Build(build, handler)

    /// Provide a Func delegate for the endpoints. You can only call this one time.
    [<CustomOperation "handle">]
    member inline this.handle([<InlineIfLambda>] build: BuildEndpoint, handler: Func<_, _, _, _>) = this.Build(build, handler)

    /// Provide a Func delegate for the endpoints. You can only call this one time.
    [<CustomOperation "handle">]
    member inline this.handle([<InlineIfLambda>] build: BuildEndpoint, handler: Func<_, _, _, _, _>) = this.Build(build, handler)

    /// Provide a Func delegate for the endpoints. You can only call this one time.
    [<CustomOperation "handle">]
    member inline this.handle([<InlineIfLambda>] build: BuildEndpoint, handler: Func<_, _, _, _, _, _>) = this.Build(build, handler)

    /// Provide a Func delegate for the endpoints. You can only call this one time.
    [<CustomOperation "handle">]
    member inline this.handle([<InlineIfLambda>] build: BuildEndpoint, handler: Func<_, _, _, _, _, _, _>) = this.Build(build, handler)
