using System;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.Internal
{
    public class PipeSchedulerAwiatable : ICriticalNotifyCompletion
    {
        private readonly PipeScheduler _ioScheduler;

        public PipeSchedulerAwiatable(PipeScheduler ioScheduler)
        {
            _ioScheduler = ioScheduler;
        }

        public PipeSchedulerAwiatable GetAwaiter() => this;
        public bool IsCompleted => false;

        public void GetResult()
        {
        }

        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            _ioScheduler.Schedule(state => ((Action)state)(), continuation);
        }
    }
}
