﻿using Commands;

var components = new ComponentTree();

components.AddRange(typeof(Program).Assembly.GetExportedTypes());
components.Add(new Command((IComponentProvider provider, ConsoleContext context) =>
{
    foreach (var command in provider.Components.GetCommands())
        context.Respond(command);

}, "help"));

var provider = new ComponentProvider(components);

while (true)
    await provider.Execute(new ConsoleContext(Console.ReadLine()));