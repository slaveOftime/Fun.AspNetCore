﻿namespace Fun.AspNetCore

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Routing


type BuildRoute = delegate of group: IEndpointRouteBuilder -> RouteHandlerBuilder
type BuildEndpoint = delegate of route: RouteHandlerBuilder -> RouteHandlerBuilder
type BuildEndpoints = delegate of endpoint: RouteGroupBuilder -> RouteGroupBuilder


namespace Fun.AspNetCore.Internal

[<Struct>]
type SupportedHttpMethod = 
    | GET
    | PUT
    | POST
    | DELETE
    | PATCH