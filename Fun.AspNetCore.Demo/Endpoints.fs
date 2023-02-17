module Fun.AspNetCore.Demo.Endpoints

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Fun.Blazor
open Fun.AspNetCore


let apis =
    endpoints "api" {
        get "hi" {
            cacheOutput
            Results.Text "world"
        }
        endpoints "user" {
            get "{userId}" {
                produces typedef<User> 200
                handle UserApis.getUser
            }
            put "{userId}" {
                authorization
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


let view =
    endpoints "view" {
        // Integrate with Fun.Blazor
        get "blog-list" {
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
            view (fun (blogId: int) -> div {
                h2 { $"Blog {blogId}" }
                p { "Please give me feedback if you want." }
            })
        }
    }
