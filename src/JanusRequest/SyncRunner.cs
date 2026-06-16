using System;
using System.Threading.Tasks;

namespace JanusRequest
{
    /// <summary>
    /// Internal helper used by the synchronous extension wrappers to invoke async work
    /// without deadlocking on a captured <see cref="System.Threading.SynchronizationContext"/>.
    /// Offloads the call to the thread pool via <see cref="Task.Run{TResult}(Func{Task{TResult}})"/>,
    /// which avoids the WPF/WinForms/legacy ASP.NET deadlock pattern at the cost of one extra thread hop.
    /// </summary>
    internal static class SyncRunner
    {
        public static T Run<T>(Func<Task<T>> asyncMethod)
            => Task.Run(asyncMethod).GetAwaiter().GetResult();
    }
}
