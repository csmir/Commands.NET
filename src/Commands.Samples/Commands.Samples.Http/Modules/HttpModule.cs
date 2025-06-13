using Commands.Http;
using System.Net;

namespace Commands.Tests;

// This module demonstrates how to create a simple HTTP command module using Commands.Http.
// Examples for general module practices is available in the Samples.Core, Samples.Console and Samples.Hosting projects.
[Name("http-module")]
public sealed class HttpModule : HttpCommandModule<HttpCommandContext>
{
    // This command can be invoked via HTTP GET requests under the following URL: http://localhost:5000/http-module/ping.
    [HttpGet("get")]
    public Task<HttpResult> Get(int initialValue, Guid anotherValue)
        => HttpResult.Ok($"Hello World; {initialValue}, {anotherValue}");

    // This command can be invoked via HTTP POST requests under the following URL: http://localhost:5000/http-module/post?queryTest=123.
    // The [JsonBody] attribute indicates that the body of the request should be parsed as JSON and deserialized into a Dictionary<string, string>.
    [HttpPost("post")]
    public Task<HttpResult> Post(string queryTest, [JsonBody] Dictionary<string, string> dictionary)
    {
        dictionary["key3"] = queryTest;

        return HttpResult.Json(dictionary, HttpStatusCode.Created);
    }

    // This command can be invoked via HTTP PUT requests under the following URL: http://localhost:5000/http-module/delete.
    [HttpDelete("delete")]
    public Task<HttpResult> Delete()
        => HttpResult.Forbidden();

    // This command can be invoked via HTTP GET requests under the following URL: http://localhost:5000/http-module/dowork.
    [HttpGet("dowork")]
    public async Task DoWork()
    {
        Context.Response.Headers.Add("X-Detached-Process", "true");

        // Submit the response immediately, so the client receives it without waiting for the long-running process to complete.
        Respond(HttpResult.Ok("Doing work"));

        // Simulate a long-running process
        await Task.Delay(10000);

        // This code will not affect the response sent to the client
        Console.WriteLine("Detached process completed.");
    }
}
