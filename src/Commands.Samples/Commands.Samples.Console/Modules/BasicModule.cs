﻿using System.Diagnostics;
using System.Text;

namespace Commands.Samples;

public class BasicModule : CommandModule<ConsoleCaller>
{
    [Name("helloworld")]
    public string HelloWorld()
    {
        return "Hello, world!";
    }

    [Name("reply")]
    public string Reply([Remainder] string message)
    {
        return "Hi, " + Caller.Name + ". " + message + "!";
    }

    [Name("type-info", "typeinfo", "type")]
    public void TypeInfo(Type type)
    {
        var response = new StringBuilder();

        response.AppendLine($"Information about: {type.Name}");
        response.AppendLine($"Fullname: {type.FullName}");
        response.AppendLine($"Assembly: {type.Assembly.FullName}");

        Respond(response.ToString());
    }

    [Name("copy")]
    [RequireOperatingSystem(PlatformID.Win32NT)]
    public string Copy([Remainder] string toCopy)
    {
        Process clipboardExecutable = new()
        {
            StartInfo = new ProcessStartInfo
            {
                RedirectStandardInput = true,
                FileName = "clip",
            }
        };
        clipboardExecutable.Start();

        clipboardExecutable.StandardInput.Write(toCopy);
        clipboardExecutable.StandardInput.Close();

        return "Succesfully copied the content to your clipboard.";
    }
}