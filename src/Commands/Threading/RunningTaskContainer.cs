using System.Timers;

namespace Commands.Threading
{
    /// <summary>
    ///     Represents a container for all running tasks that are not held by reference in synchronous execution.
    /// </summary>
    public sealed class RunningTaskContainer : IDisposable
    {
        private readonly System.Timers.Timer _cleanupTimer;

        private readonly Dictionary<Guid, Task> _taskRef = [];
        
        private readonly object _lock = new();

        internal RunningTaskContainer()
        {
            _cleanupTimer = new(100)
            {
                AutoReset = true
            };

            _cleanupTimer.Elapsed += CleanInvoker;
            _cleanupTimer.Start();
        }

        private void CleanInvoker(object? _, ElapsedEventArgs arg)
        {
            lock (_lock)
            {

            }
        }

        /// <summary>
        ///     Disposes of the container and all running tasks.
        /// </summary>
        public void Dispose()
        {

        }
    }
}
