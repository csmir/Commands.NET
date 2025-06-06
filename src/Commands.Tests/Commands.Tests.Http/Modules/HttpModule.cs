using Commands.Http;
using System.Net;

namespace Commands.Tests;

[Name("http-module")]
public sealed class HttpModule : HttpCommandModule<HttpCommandContext>
{
    [HttpGet]
    [Name("get")]
    public Task<HttpResponse> Get(int initialValue, Guid anotherValue)
        => HttpResponse.Ok($"Hello World; {initialValue}, {anotherValue}");

    [HttpPost]
    [Name("post")]
    public Task<HttpResponse> Post(int initialValue, Guid anotherValue)
        => HttpResponse.Json(new { InitialValue = initialValue, AnotherValue = anotherValue }, HttpStatusCode.Created);

    [HttpDelete]
    [Name("delete")]
    public Task<HttpResponse> Delete()
        => HttpResponse.Forbidden();

    [HttpGet]
    [Name("dowork")]
    public async Task DoWork()
    {
        Context.Response.Headers.Add("X-Detached-Process", "true");

        Respond(HttpResponse.Ok("Doing work"));

        // Simulate a long-running process
        await Task.Delay(10000);

        // This code will not affect the response sent to the client
        Console.WriteLine("Detached process completed.");
    }
}
