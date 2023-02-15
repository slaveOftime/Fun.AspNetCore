#nowarn "0020"

open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Fun.AspNetCore


type User = { Id: int; Name: string}


let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())

builder.Services.AddEndpointsApiExplorer()
builder.Services.AddSwaggerGen()


let app = builder.Build()

if app.Environment.IsDevelopment() then
    app.UseSwagger()
    app.UseSwaggerUI() |> ignore

app.MapFunEndpoints(endpoints "api" {
    config (fun route -> route.WithName(""))

    //GET "hi" (fun () -> "world")
    
    endpoints "user" {
        get "{userId}" {
            authorization
            produces typeof<User> 200
            config (fun r -> r.CacheOutput())
            handle (fun (userId: int) -> { Id = userId; Name = "Foo" })
        }
        put "{userId}" {
            authorization
            handle (fun _ -> Results.Ok "Updated")
        }
    }
})

app.Run()
