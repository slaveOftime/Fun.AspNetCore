namespace Fun.AspNetCore.Internal

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Routing


type BuildRoute = delegate of group: IEndpointRouteBuilder -> RouteHandlerBuilder
type BuildGroup = delegate of group: IEndpointRouteBuilder -> RouteGroupBuilder
type BuildEndpoint = delegate of route: RouteHandlerBuilder -> RouteHandlerBuilder
type BuildEndpoints = delegate of endpoint: RouteGroupBuilder -> RouteGroupBuilder
