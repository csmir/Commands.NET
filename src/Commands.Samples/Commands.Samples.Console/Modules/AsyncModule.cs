﻿namespace Commands.Samples.Console.Modules
{
    public class AsyncModule : ModuleBase
    {
        [Name("task-string")]
        public Task<string> GetString()
        {
            return Task.FromResult("Hello from a task command.");
        }

        [Name("task-empty")]
        public Task GetEmpty()
        {
            return Send("Hello from a task command with no return value.");
        }
    }
}
