namespace Fun.AspNetCore.Demo

open System


type User = { Id: int; Name: string }


module UserApis =
    // You can declare the handler in other places instead of inline it in CE
    // But, we have to use Func<...> to make fsharp compiled IL will not change the argument name when implicit convert fsharp func to Func delegate
    let getUser = Func<int, _>(fun userId -> { Id = userId; Name = "Foo" })
