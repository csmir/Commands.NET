﻿namespace Commands.Tests
{
    public sealed class AsyncModule : ModuleBase<ConsumerBase>
    {
        [Name("async")]
        public async Task Async(bool delay)
        {
            if (delay)
            {
                await Task.Delay(Random.Shared.Next(5000, 10000));

                await Send("Success");
            }
        }
    }
}
