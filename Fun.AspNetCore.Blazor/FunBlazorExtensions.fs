[<AutoOpen>]
module Fun.AspNetCore.Extensions

open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Fun.Blazor


type Results with

    static member inline View(node: NodeRenderFragment) =
        { new IResult with
            member _.ExecuteAsync(ctx) = ctx.WriteFunDom(node)
        }


type EndpointCEBuilder with

    member inline this.Yield([<InlineIfLambda>] node: NodeRenderFragment) =
        BuildRoute(fun group -> group.MapMethods(this.Pattern, this.Methods, Func<_, _>(fun (ctx: HttpContext) -> ctx.WriteFunDom(node))))



//[<CustomOperation "view">]
//member inline this.view([<InlineIfLambda>] build: BuildEndpoint, handler: Func<'T, NodeRenderFragment>) =
//    BuildRoute(fun group ->
//        let route = group.MapMethods(this.Pattern, this.Methods, handler)
//        route.Add(fun b -> b.Metadata.Add({ new IFunBlazorNodeMeta }))
//        build.Invoke(route)
//    )

//[<CustomOperation "view">]
//member inline this.view([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] makeView: _ * _ -> NodeRenderFragment) =
//    this.Build(build, Func<_, _, _, _>(fun (ctx: HttpContext) x1 x2 -> ctx.WriteFunDom(makeView (x1, x2))))

//[<CustomOperation "view">]
//member inline this.view([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] makeView: _ * _ * _ -> NodeRenderFragment) =
//    this.Build(build, Func<_, _, _, _, _>(fun (ctx: HttpContext) x1 x2 x3 -> ctx.WriteFunDom(makeView (x1, x2, x3))))

//[<CustomOperation "view">]
//member inline this.view([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] makeView: _ * _ * _ * _ -> NodeRenderFragment) =
//    this.Build(build, Func<_, _, _, _, _, _>(fun (ctx: HttpContext) x1 x2 x3 x4 -> ctx.WriteFunDom(makeView (x1, x2, x3, x4))))

//[<CustomOperation "view">]
//member inline this.view([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] makeView: _ * _ * _ * _ * _ -> NodeRenderFragment) =
//    this.Build(build, Func<_, _, _, _, _, _, _>(fun (ctx: HttpContext) x1 x2 x3 x4 x5 -> ctx.WriteFunDom(makeView (x1, x2, x3, x4, x5))))

//[<CustomOperation "view">]
//member inline this.view([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] makeView: _ * _ * _ * _ * _ * _ -> NodeRenderFragment) =
//    this.Build(
//        build,
//        Func<_, _, _, _, _, _, _, _>(fun (ctx: HttpContext) x1 x2 x3 x4 x5 x6 -> ctx.WriteFunDom(makeView (x1, x2, x3, x4, x5, x6)))
//    )

//[<CustomOperation "view">]
//member inline this.view([<InlineIfLambda>] build: BuildEndpoint, [<InlineIfLambda>] makeView: _ * _ * _ * _ * _ * _ * _ -> NodeRenderFragment) =
//    this.Build(
//        build,
//        Func<_, _, _, _, _, _, _, _, _>(fun (ctx: HttpContext) x1 x2 x3 x4 x5 x6 x7 -> ctx.WriteFunDom(makeView (x1, x2, x3, x4, x5, x6, x7)))
//    )
