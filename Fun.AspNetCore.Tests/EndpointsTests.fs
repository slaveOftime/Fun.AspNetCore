module Fun.AspNetCore.Tests.EndpointsTests

open System.Text
open System.Net
open System.Net.Http
open Microsoft.AspNetCore.Mvc.Testing
open Xunit


type private DemoApplication() =
    inherit WebApplicationFactory<Fun.AspNetCore.Demo.User>()


let private client = (new DemoApplication()).CreateClient()


[<Fact>]
let ``Simple get api should world`` () = task {
    let! actual = client.GetStringAsync("/api/hi")
    Assert.Equal("world", actual)
}

[<Fact>]
let ``Nested api should work`` () = task {
    let! actual = client.GetStringAsync("/api/account/login")
    Assert.Equal("logged in", actual)
}

[<Fact>]
let ``Auth should work`` () = task {
    let! actual = client.GetAsync("/api/user/123")
    Assert.Equal(HttpStatusCode.OK, actual.StatusCode)

    use content = new StringContent("", Encoding.UTF8, "application/json")
    let! actual = client.PutAsync("/api/user/123", content)
    Assert.Equal(HttpStatusCode.NotFound, actual.StatusCode)
}

[<Fact>]
let ``Fun Blazor integration should work`` () = task {
    let! actual = client.GetStringAsync("/view/blog-list")
    Assert.Equal("""<div class="blog-list my-5"><a href="/view/blog/1">blog 1</a><a href="/view/blog/2">blog 2</a></div>""", actual)

    let! actual = client.GetStringAsync("/view/blog/1")
    Assert.Equal("logged in", actual)
}