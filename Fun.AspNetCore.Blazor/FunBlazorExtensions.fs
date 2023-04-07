[<AutoOpen>]
module Fun.AspNetCore.Extensions

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc.Rendering
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Fun.Blazor
open Fun.AspNetCore.Internal


type Results with

    [<Obsolete("Please use enableFunBlazor with related CE")>]
    static member inline View(node: NodeRenderFragment) =
        { new IResult with
            member _.ExecuteAsync(ctx) = ctx.WriteFunDom(node)
        }


type FunBlazorEndpointFilter(renderMode) =
    interface IEndpointFilter with
        member _.InvokeAsync(ctx, next) =
            task {
                match! next.Invoke(ctx) with
                | :? NodeRenderFragment as node ->
                    return
                        { new IResult with
                            member _.ExecuteAsync(ctx) = ctx.WriteFunDom(node, renderMode)
                        }
                        :> obj

                | x -> return x
            }
            |> ValueTask<obj>


type EndpointCEBuilder with

    member inline this.Yield([<InlineIfLambda>] node: NodeRenderFragment) =
        BuildRoute(fun group -> group.MapMethods(this.Pattern, this.Methods, Func<_>(fun () -> node)))


    [<CustomOperation "enableFunBlazor">]
    member inline _.enableFunBlazor([<InlineIfLambda>] build: BuildEndpoint, ?renderMode) =
        BuildEndpoint(fun routeBuilder ->
            build
                .Invoke(routeBuilder)
                .Produces(200, "text/html")
                .AddEndpointFilter(FunBlazorEndpointFilter(defaultArg renderMode RenderMode.Static))
        )


type EndpointsCEBuilder with

    [<CustomOperation "enableFunBlazor">]
    member inline _.enableFunBlazor([<InlineIfLambda>] build: BuildEndpoints, ?renderMode) =
        BuildEndpoints(fun routeGroupBuilder ->
            build.Invoke(routeGroupBuilder).AddEndpointFilter(FunBlazorEndpointFilter(defaultArg renderMode RenderMode.Static))
        )
