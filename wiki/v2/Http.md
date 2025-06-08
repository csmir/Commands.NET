Commands.NET allows you to define HTTP (REST API) endpoints just as easily as in the core package, by using the already defined rules and API's available by default.
The HTTP package extends the core library with additional features to support HTTP commands, such as routing, request handling, and response formatting.

- [Package Download](#download)
- [Configure the Host](#configure-the-host)
- [Endpoints](#endpoints)
- [Input Definitions](#input-definitions)
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

HTTP execution builds upon the hosted command pipeline available in the `Commands.NET.Hosting` package.

In order to begin configuring, an API is available in the `Commands.Http` namespace, which can be used to configure the HTTP server and the integration with Commands.NET.
All configuration as available in the `Commands.Hosting` namespace is also available here, so you can use the same methods to configure the complete pipeline.

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

When defining commands, the `HttpMethodAttribute` or any of its derived attributes can be used to specify the HTTP method for the command.

### GET

```csharp
[Name("http-module")]
public sealed class HttpModule : HttpCommandModule<HttpCommandContext>
{
    [HttpGet("get")]
    public Task<HttpResponse> Get(int initialValue, Guid anotherValue)
        => HttpResponse.Ok($"Hello World; {initialValue}, {anotherValue}");
}
```

This example defines a command that responds to `GET` requests at the endpoint `http://localhost:5000/http-module/get`. The parameters `initialValue` and `anotherValue` will be parsed from the url.

For example, you can call `http://localhost:5000/http-module/get?initialValue=42&anotherValue=123e4567-e89b-12d3-a456-426614174000` to get a response, 
or do so using snailing: `http://localhost:5000/http-module/get/42/123e4567-e89b-12d3-a456-426614174000`.

### POST

```csharp
[Name("http-module")]
public sealed class HttpModule : HttpCommandModule<HttpCommandContext>
{
    [HttpPost("post")]
    public Task<HttpResponse> Post([JsonBody] string content)
        => HttpResponse.Ok($"Received: {content}");
}
```

This example defines a command that responds to `POST` requests at the endpoint `http://localhost:5000/http-module/post`. The body of the request will be parsed as JSON and passed to the command.

The `[JsonBody]` attribute indicates that the content of the request body should be deserialized from JSON into the `string content` parameter.

> [!TIP]
> The `[JsonBody]` attribute can be used with any type. It is available at any position in the command signature, and will attempt to deserialize the request body into the specified type. 
> If no request body is provided and the type is not nullable, an error will be returned.

### Other HTTP Methods

PUT, DELETE and PATCH are supported in the same way as GET and POST, using the respective attributes: `HttpPut`, `HttpDelete`, and `HttpPatch`.

It is also possible to define methods not bound to any standard HTTP method by using the `HttpMethodAttribute` directly, allowing you to specify any HTTP method you desire.

```csharp
[Name("http-module")]
public sealed class HttpModule : HttpCommandModule<HttpCommandContext>
{
    [HttpMethod("CUSTOM", "custom")]
    public Task<HttpResponse> CustomMethod()
        => HttpResponse.Ok("This is a custom HTTP method response.");
}
```

## Response Formatting

When a command returns an `HttpResult` or `IHttpResult`, it will be sent directly to the caller. 
If the command returns a different type, it will be converted to an `HttpResult` using the default response formatter.

`HttpResult` has a set of static methods that can be used to create responses with different status codes and content types, including JSON formatting.