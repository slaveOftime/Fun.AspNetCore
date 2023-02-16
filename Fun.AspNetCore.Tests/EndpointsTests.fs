module Fun.AspNetCore.Tests.EndpointsTests

open System.Net
open Xunit
open Microsoft.AspNetCore.Mvc.Testing


type DemoApplication() =
    inherit WebApplicationFactory<Fun.AspNetCore.Demo.Program.User>()


let client = (new DemoApplication()).CreateClient()


[<Fact>]
let ``Simple get api should world`` () = task {
    let! actual = client.GetStringAsync("/api/hi")
    Assert.Equal("\"world\"", actual)
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
}
