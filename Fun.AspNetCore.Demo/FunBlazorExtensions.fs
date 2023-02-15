[<AutoOpen>]
module Fun.Blazor.Extensions

open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Mvc.Rendering
open Fun.Blazor
open Fun.AspNetCore


type EndpointCEBuilder with

    member inline _.Yield([<InlineIfLambda>] node: NodeRenderFragment) = node

    member inline this.Delay([<InlineIfLambda>] fn: unit -> NodeRenderFragment) =
        BuildRoute(fun group ->
            let route =
                group.MapMethods(this.Pattern, this.Methods, Func<_, _>(fun (ctx: HttpContext) -> ctx.WriteFunDom(fn (), RenderMode.Static)))
            route
        )

    member inline this.Combine([<InlineIfLambda>] node: NodeRenderFragment, _: BuildRoute) =
        BuildRoute(fun group ->
            let route =
                group.MapMethods(this.Pattern, this.Methods, Func<_, _>(fun (ctx: HttpContext) -> ctx.WriteFunDom(node, RenderMode.Static)))
            route
        )

    [<CustomOperation "view">]
    member inline this.view([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] makeView: 'T -> NodeRenderFragment) =
        if typeof<'T> = typeof<unit> then
            this.Build(build, Func<_, _>(fun (ctx: HttpContext) -> ctx.WriteFunDom(makeView (unbox<'T> ()))))
        else
            this.Build(build, Func<_, _, _>(fun (ctx: HttpContext) x -> ctx.WriteFunDom(makeView x)))

    [<CustomOperation "view">]
    member inline this.view([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] makeView: _ * _ -> NodeRenderFragment) =
        this.Build(build, Func<_, _, _, _>(fun (ctx: HttpContext) x1 x2 -> ctx.WriteFunDom(makeView (x1, x2))))

    [<CustomOperation "view">]
    member inline this.view([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] makeView: _ * _ * _ -> NodeRenderFragment) =
        this.Build(build, Func<_, _, _, _, _>(fun (ctx: HttpContext) x1 x2 x3 -> ctx.WriteFunDom(makeView (x1, x2, x3))))

    [<CustomOperation "view">]
    member inline this.view([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] makeView: _ * _ * _ * _ -> NodeRenderFragment) =
        this.Build(build, Func<_, _, _, _, _, _>(fun (ctx: HttpContext) x1 x2 x3 x4 -> ctx.WriteFunDom(makeView (x1, x2, x3, x4))))

    [<CustomOperation "view">]
    member inline this.view([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] makeView: _ * _ * _ * _ * _ -> NodeRenderFragment) =
        this.Build(build, Func<_, _, _, _, _, _, _>(fun (ctx: HttpContext) x1 x2 x3 x4 x5 -> ctx.WriteFunDom(makeView (x1, x2, x3, x4, x5))))

    [<CustomOperation "view">]
    member inline this.view([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] makeView: _ * _ * _ * _ * _ * _ -> NodeRenderFragment) =
        this.Build(
            build,
            Func<_, _, _, _, _, _, _, _>(fun (ctx: HttpContext) x1 x2 x3 x4 x5 x6 -> ctx.WriteFunDom(makeView (x1, x2, x3, x4, x5, x6)))
        )

    [<CustomOperation "view">]
    member inline this.view([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] makeView: _ * _ * _ * _ * _ * _ * _ -> NodeRenderFragment) =
        this.Build(
            build,
            Func<_, _, _, _, _, _, _, _, _>(fun (ctx: HttpContext) x1 x2 x3 x4 x5 x6 x7 -> ctx.WriteFunDom(makeView (x1, x2, x3, x4, x5, x6, x7)))
        )
