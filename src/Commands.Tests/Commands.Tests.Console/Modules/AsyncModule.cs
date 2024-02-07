using Commands.Core;

namespace Commands.Tests
{
    public sealed class AsyncModule : ModuleBase<ConsumerBase>
    {
        [Command("async")]
        public async Task Async(bool delay)
        {
            if (delay)
            {
                await Task.Delay(Random.Shared.Next(100, 1000));
            }
        }
    }
}
