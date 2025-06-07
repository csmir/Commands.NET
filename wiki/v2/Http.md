Commands.NET allows you to define HTTP (REST API) endpoints just as easily as in the core package, by using the already defined rules and API's available by default.
The HTTP package extends the core library with additional features to support HTTP commands, such as routing, request handling, and response formatting.

- [Package Download](#download)
- [Configure the Host](#configure-the-host)
- [Endpoints](#endpoints)
- [Response Formatting](#response-formatting)

## Download

The HTTP package is available on NuGet. You can install it using the package manager, or the following command:

```bash
dotnet add package Commands.NET.Http
```

Alternatively, adding it to your `.csproj` file:

```xml
<PackageReference Include="Commands.NET.Http" Version="x.x.x" />
```

## Configure the Host

HTTP execution builds upon the Hosted command pipeline. 
In order to begin configuring, a new API is available in the `Commands.Http` namespace, which can be used to configure the HTTP server and the integration with Commands.NET.
All configuration as available in the `Commands.Hosting` package is also available here, so you can use the same method to configure the complete hosted pipeline.

```csharp
// ...

builder.ConfigureHttpComponents(context =>
{
    context.ConfigureListener(listener =>
    {
        listener.Prefixes.Add("http://localhost:5000/");
    });
});
```

> [!IMPORTANT]
> The `ConfigureListener` method is required to set the prefixes for the HTTP server, and cannot be omitted. 
> Call this API before building the host.

Just like when using `Commands.Hosting`, after building the host, `UseComponents` can be called to register commands which will be considered for API routing.

```csharp
// ...

var host = builder.Build();

host.UseComponents(components => 
{
    components.Add(new Command(() =>
    {
        return HttpResponse.Ok("OK!");
    }, "ping"));
});

host.Run();
```

With this setup, the command `ping` will be available at the endpoint `http://localhost:5000/ping`. You can start the application and test it using a web browser or a tool like Postman.

## Endpoints

When defining commands, the `HttpMethodAttribute` or any of its derived attributes can be used to specify the HTTP method for the command. If none is specified, it will accept all HTTP methods.
This works in conjunction with the `NameAttribute` to define the endpoint path.

```csharp
[Name("http-module")]
public sealed class HttpModule : HttpCommandModule<HttpCommandContext>
{
    [HttpGet]
    [Name("get")]
    public Task<HttpResponse> Get(int initialValue, Guid anotherValue)
        => HttpResponse.Ok($"Hello World; {initialValue}, {anotherValue}");
}
```

This example defines a command that responds to `GET` requests at the endpoint `http://localhost:5000/http-module/get`. The parameters `initialValue` and `anotherValue` will be parsed from the url.

For example, you can call `http://localhost:5000/http-module/get?initialValue=42&anotherValue=123e4567-e89b-12d3-a456-426614174000` to get a response.

Or, do so using snailing: `http://localhost:5000/http-module/get/42/123e4567-e89b-12d3-a456-426614174000`.

## Response Formatting

When a command returns an `HttpResponse` or `IHttpResult`, it will be sent directly to the caller. If the command returns a different type, it will be converted to an `HttpResponse` using the default response formatter.

`HttpResponse` has a set of static methods that can be used to create responses with different status codes and content types:

```csharp
HttpResponse.Ok("Success!"); // 200 OK
HttpResponse.BadRequest("Invalid request"); // 400 Bad Request
HttpResponse.NotFound("Resource not found"); // 404 Not Found
HttpResponse.InternalServerError("An error occurred"); // 500 Internal Server Error
```