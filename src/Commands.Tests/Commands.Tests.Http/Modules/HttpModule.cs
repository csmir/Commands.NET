using Commands.Http;
using System.Net;

namespace Commands.Tests;

[Name("http-module")]
public sealed class HttpModule : HttpCommandModule<HttpCommandContext>
{
    [HttpGet]
    [Name("simple-get")]
    public Task<HttpResponse> SimpleGet(int initialValue, Guid anotherValue)
    {
        return Ok("Hello World");
    }

    [HttpPost]
    [Name("simple-post")]
    public Task<HttpResponse> SimplePost(int initialValue, Guid anotherValue)
    {
        return Json("Hello World", HttpStatusCode.Created);
    }

    [HttpDelete]
    [Name("simple-delete")]
    public Task<HttpResponse> SimpleDelete(int initialValue, Guid anotherValue)
    {
        return Forbidden();
    }

    [HttpGet]
    [Name("dowork")]
    public async Task SimpleDetachedProcess()
    {
        SetResponseHeader("X-ProcessDuration-MS", "10000");
        SetResponse(Ok("Doing work"));

        Respond();

        // Simulate a long-running process
        await Task.Delay(10000);

        // This code will not affect the response sent to the client
        Console.WriteLine("Detached process completed.");
    }
}
