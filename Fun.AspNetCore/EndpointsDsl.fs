namespace Fun.AspNetCore

open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Routing


[<Extension>]
type EndpointsExtensions =

    [<Extension>]
    static member inline MapGroup(this: IEndpointRouteBuilder, [<InlineIfLambda>] build: BuildGroup) = build.Invoke(this)


[<AutoOpen>]
module EndpointsDsl =

    let inline typedef<'T> () = typeof<'T>

    let inline get pattern = EndpointCEBuilder(EndpointCEBuilder.GetMethods, pattern)
    let inline put pattern = EndpointCEBuilder(EndpointCEBuilder.PutMethods, pattern)
    let inline post pattern = EndpointCEBuilder(EndpointCEBuilder.PostMethods, pattern)
    let inline delete pattern = EndpointCEBuilder(EndpointCEBuilder.DeleteMethods, pattern)
    let inline patch pattern = EndpointCEBuilder(EndpointCEBuilder.PatchMethods, pattern)

    let inline endpoints pattern = EndpointsCEBuilder pattern
