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
    Assert.Equal(HttpStatusCode.NotFound, actual.StatusCode)

    use content = new StringContent("""{ "Id": 123, "Name": "foo" }""", Encoding.UTF8, "application/json")
    let! result = client.PutAsync("/api/user/123", content)
    let! actual = result.Content.ReadAsStringAsync()
    Assert.Equal("Updated: 123 foo", actual)
}

[<Fact>]
let ``Fun Blazor integration should work`` () = task {
    let! actual = client.GetStringAsync("/view/blog-list")
    Assert.Equal("""<div class="blog-list my-5"><a href="/view/blog/1">blog 1</a><a href="/view/blog/2">blog 2</a></div>""", actual)

    let! actual = client.GetStringAsync("/view/blog/1")
    Assert.Equal("""<div><h2>Blog 1</h2><p>Please give me feedback if you want.</p></div>""", actual)
}
