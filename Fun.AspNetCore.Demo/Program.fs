#nowarn "0020"

open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.Extensions.DependencyInjection
open Fun.AspNetCore
open Fun.AspNetCore.Demo


let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())
let services = builder.Services

services.AddEndpointsApiExplorer()
services.AddSwaggerGen()
services.AddControllersWithViews()
services.AddOutputCache()
services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie()
services.AddAuthorization()


let app = builder.Build()

if app.Environment.IsDevelopment() then
    app.UseSwagger()
    app.UseSwaggerUI() |> ignore

app.UseOutputCache()
app.UseAuthentication()
app.UseAuthorization()

app.MapGroup(Endpoints.apis)
app.MapGroup(Endpoints.view)

app.MapGroup("normal").MapGet("hi", Func<_>(fun () -> Results.Text "world"))

app.Run()
