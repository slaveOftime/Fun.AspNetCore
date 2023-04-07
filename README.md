# Fun.AspNetCore [![Nuget](https://img.shields.io/nuget/vpre/Fun.AspNetCore)](https://www.nuget.org/packages/Fun.AspNetCore)

This is a experimental project for provide a very thin layer on AspNetCore minimal api for fsharp developers who love CE syntax (❤).

The reason to call it thin layer is because pwoered by the fsharp inline, a lot of overhead will be removed and what it actually compiled is what you may write by using the raw api manually.

There is a convention for using it:

**endpoints** is a group of endpoint, it can contain nested **endpoints** or get/put/post/delete/patch endpoints etc.

```fsharp
endpoints "api" {
    // the settings like authorization, goes first
    
    // nested endpoints
    endpoints "user" {
        ...
    }

    // single endpoint
    get "hi" { ... }
}
```

For a single endpoint it also follow similar pattern

```fsharp
get "hi" {
    // the settings like authorization, goes first

    // handle should put in the last
    handle (fun (v1: T1) (v2: T2) ... -> ...)
    // The function argumentS should not be tuples
    // You can use function which is defined in other places, but it must be defined as Func<_, _>(fun (v1: T1) (v2: T2) ... -> ...).
    // Like: let getUser = Func<int, User>(fun userId -> { Id = userId; Name = "foo" })
    // The different with csharp minimal api is: you can not add attribute to the argument because of fsharp limitation.

    // You can also yield IResult and NodeRenderFragment(for Fun.Blazor) without use handle, they are special
}
```


## Fun.AspNetCore example

```fsharp
...
let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())
let services = builder.Services
...
let app = builder.Build()
...

app.MapGroup(
    endpoints "api" {
        get "hi" { Results.Text "world" }
        // You can nest endpoints
        endpoints "user" {
            get "{userId}" {
                authorization
                produces typedef<User> 200
                producesProblem 404
                handle UserApis.getUser
            }
            put "{userId}" {
                // You can access all apis provided by AspNetCore by use set operation
                set (fun route -> route.Accepts("application/json").WithName("foo"))
                handle (fun (userId: int) (user: User) -> Results.Text $"Updated: {userId} {user.Name}")
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
...
```

## Fun.AspNetCore.Blazor [![Nuget](https://img.shields.io/nuget/vpre/Fun.AspNetCore.Blazor)](https://www.nuget.org/packages/Fun.AspNetCore.Blazor) example

```fsharp
...
let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())
let services = builder.Services
...
services.AddControllersWithViews() // Will register some service for writing dom into response
...
let app = builder.Build()
...

app.MapGroup(
    endpoints "view" {
        // Integrate with Fun.Blazor
        enableFunBlazor

        get "time" {
            // You can enable for a specific route
            enableFunBlazor
            cacheOutput (fun b -> b.Expire(TimeSpan.FromSeconds 5))
            div { $"{DateTime.Now}" }
        }
        get "blog-list" {
            produces 200 "text/html"
            div {
                class' "blog-list my-5"
                childContent [
                    for i in 1..2 do
                        a {
                            href $"/view/blog/{i}"
                            $"blog {i}"
                        }
                ]
            }
        }
        get "blog/{blogId}" {
            produces 200 "text/html"
            handle (fun (blogId: int) -> div {
                h2 { $"Blog {blogId}" }
                p { "Please give me feedback if you want." }
            })
        }
    }
)
...
```
