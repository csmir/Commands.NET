using Commands.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Commands.Tests
{
    public sealed class Module(IServiceProvider provider) : ModuleBase
    {
        private readonly IServiceProvider _provider = provider;

        [Command("stop")]
        public void Stop()
        {
            var handler = _provider.GetRequiredService<CommandHandler>();

            handler.StopAsync(default);
        }
    }
}
