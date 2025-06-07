using Commands.Http;
using System.Net;

namespace Commands.Tests;

[Name("http-module")]
public sealed class HttpModule : HttpCommandModule<HttpCommandContext>
{
    [HttpGet("get")]
    public Task<HttpResult> Get(int initialValue, Guid anotherValue)
        => HttpResult.Ok($"Hello World; {initialValue}, {anotherValue}");

    [HttpPost("post")]
    public Task<HttpResult> Post(string queryTest, [JsonBody] Dictionary<string, string> dictionary)
    {
        dictionary["key3"] = queryTest;

        return HttpResult.Json(dictionary, HttpStatusCode.Created);
    }

    [HttpDelete("delete")]
    public Task<HttpResult> Delete()
        => HttpResult.Forbidden();

    [HttpGet("dowork")]
    public async Task DoWork()
    {
        Context.Response.Headers.Add("X-Detached-Process", "true");

        Respond(HttpResult.Ok("Doing work"));

        // Simulate a long-running process
        await Task.Delay(10000);

        // This code will not affect the response sent to the client
        Console.WriteLine("Detached process completed.");
    }
}
