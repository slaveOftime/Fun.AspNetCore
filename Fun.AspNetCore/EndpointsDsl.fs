namespace Fun.AspNetCore

open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Routing
open Microsoft.AspNetCore.Builder
open Fun.AspNetCore.Internal


[<Extension>]
type EndpointsExtensions =

    [<Extension>]
    static member MapFunEndpoints(this: IEndpointRouteBuilder, build: BuildEndpoints) =  build.Invoke(this.MapGroup(""))
        

[<AutoOpen>]
module EndpointsDsl =

    let inline get pattern = EndpointCEBuilder(GET, pattern)
    let inline put pattern = EndpointCEBuilder(PUT, pattern)
    let inline post pattern = EndpointCEBuilder(POST, pattern)
    let inline delete pattern = EndpointCEBuilder(DELETE, pattern)
    let inline patch pattern = EndpointCEBuilder(PATCH, pattern)

    let inline endpoints pattern = EndpointsCEBuilder pattern
