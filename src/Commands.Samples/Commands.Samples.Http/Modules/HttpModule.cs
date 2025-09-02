using Commands.Http;
using System.Net;

namespace Commands.Tests;

// This module demonstrates how to create a simple HTTP command module using Commands.Http.
// Examples for general module practices is available in the Samples.Core, Samples.Console and Samples.Hosting projects.
[Name("http")]
public sealed class HttpModule : HttpCommandModule<HttpCommandContext>
{
    // This command can be invoked via HTTP GET requests under the following URL: http://localhost:5000/http/ping.
    [HttpGet("get")]
    public IHttpResult Get(int initialValue, Guid anotherValue)
        => Ok($"Hello World; {initialValue}, {anotherValue}");

    // This command can be invoked via HTTP POST requests under the following URL: http://localhost:5000/http/post?queryTest=123.
    // The [JsonBody] attribute indicates that the body of the request should be parsed as JSON and deserialized into a Dictionary<string, string>.
    [HttpPost("post")]
    public IHttpResult Post(string queryTest, [JsonBody] Dictionary<string, string> dictionary)
    {
        dictionary["key3"] = queryTest;

        return Json(dictionary, HttpStatusCode.Created);
    }

    // This command can be invoked via HTTP PUT requests under the following URL: http://localhost:5000/http/delete.
    [HttpDelete("delete")]
    public IHttpResult Delete()
        => Forbidden();

    // This command can be invoked via HTTP GET requests under the following URL: http://localhost:5000/http/dowork.
    [HttpGet("dowork")]
    public async Task DoWork()
    {
        var response = Ok("Doing work").WithHeader("X-Detached", "true");

        // Submit the response immediately, so the client receives it without waiting for the long-running process to complete.
        Respond(response);

        // Simulate a long-running process
        await Task.Delay(10000);

        // This code will not affect the response sent to the client
        Console.WriteLine("Detached process completed.");
    }
}
