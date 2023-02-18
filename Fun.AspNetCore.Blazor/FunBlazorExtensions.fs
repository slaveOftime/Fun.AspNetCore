[<AutoOpen>]
module Fun.AspNetCore.Extensions

open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Fun.Blazor
open Fun.AspNetCore.Internal


type Results with

    static member inline View(node: NodeRenderFragment) =
        { new IResult with
            member _.ExecuteAsync(ctx) = ctx.WriteFunDom(node)
        }


type EndpointCEBuilder with

    member inline this.Yield([<InlineIfLambda>] node: NodeRenderFragment) =
        BuildRoute(fun group -> group.MapMethods(this.Pattern, this.Methods, Func<_, _>(fun (ctx: HttpContext) -> ctx.WriteFunDom(node))))
