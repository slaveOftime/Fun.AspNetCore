module Fun.AspNetCore.Demo.Program

open Microsoft.AspNetCore.Authentication.Cookies

#nowarn "0020"

open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Fun.Blazor
open Fun.AspNetCore


type User = { Id: int; Name: string }

let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())
let services = builder.Services

services.AddEndpointsApiExplorer()
services.AddSwaggerGen()
services.AddControllersWithViews()
services.AddFunBlazorServer()
services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie()
services.AddAuthorization()


let app = builder.Build()

if app.Environment.IsDevelopment() then
    app.UseSwagger()
    app.UseSwaggerUI() |> ignore

app.UseAuthentication()
app.UseAuthorization()

app.MapGroup(
    endpoints "api" {
        get "hi" {
            cacheOutput
            Results.Ok "world"
        }
        endpoints "user" {
            authorization
            get "{userId}" {
                produces typedef<User> 200
                handle (fun (userId: int) -> { Id = userId; Name = "Foo" })
            }
            put "{userId}" {
                producesProblem 404
                // You can access all apis provided by AspNetCore by use set operation
                set (fun route -> route.Accepts("application/json").WithName("foo"))
                handle (fun () -> Results.Ok "Updated")
            }
        }
        endpoints "account" {
            anonymous
            get "login" { handle (fun () -> "logged in") }
        }
        endpoints "security" {
            authorization
            tags "high-security"
            get "money" { handle (fun () -> "world") }
            put "money" { handle (fun () -> "world") }
        }
    }
)

app.MapGroup(
    endpoints "view" {
        // Integrate with Fun.Blazor
        get "blog-list" {
            div {
                class' "blog-list my-5"
                childContent [
                    for i in 1..5 do
                        a {
                            href $"/view/blog/{i}"
                            $"blog {i}"
                        }
                ]
            }
        }
        get "blog/{blogId}" {
            view (fun (blogId: int) -> div {
                h2 { $"Blog {blogId}" }
                p { "Please give me feedback if you want." }
            })
        }
    }
)

app

app.Run()
